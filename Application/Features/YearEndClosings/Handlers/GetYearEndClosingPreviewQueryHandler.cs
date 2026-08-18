using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.YearEndClosings.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.YearEndClosing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.YearEndClosings.Handlers
{
    public class GetYearEndClosingPreviewQueryHandler : IRequestHandler<GetYearEndClosingPreviewQuery, YearEndClosingPreview>
    {
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;
        private readonly IRepository<JournalLineEntity> _journalLineRepository;

        public GetYearEndClosingPreviewQueryHandler(
            IRepository<ChartOfAccountEntity> coaRepository,
            IRepository<JournalLineEntity> journalLineRepository)
        {
            _coaRepository = coaRepository;
            _journalLineRepository = journalLineRepository;
        }

        public async Task<YearEndClosingPreview> Handle(GetYearEndClosingPreviewQuery request, CancellationToken cancellationToken)
        {
            var endOfYear = new DateTime(request.Request.FiscalYear, 12, 31);
            var accounts = await _coaRepository.Query()
                .Where(a => !a.IsDeleted && a.IsActive && !a.IsHeader
                    && (a.AccountType == "Revenue" || a.AccountType == "Other Income"
                        || a.AccountType == "Expense" || a.AccountType == "Other Expense"))
                .OrderBy(a => a.AccountCode)
                .ToListAsync(cancellationToken);

            var revenueAccounts = new List<YearEndClosingLinePreview>();
            var expenseAccounts = new List<YearEndClosingLinePreview>();
            decimal totalRevenue = 0;
            decimal totalExpense = 0;

            foreach (var account in accounts)
            {
                var balance = await GetAccountBalanceAsync(account, endOfYear, cancellationToken);
                if (balance == 0) continue;
                var item = new YearEndClosingLinePreview
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountCode,
                    AccountName = account.AccountName,
                    Balance = balance
                };
                if (account.AccountType is "Revenue" or "Other Income")
                {
                    revenueAccounts.Add(item);
                    totalRevenue += balance;
                }
                else
                {
                    expenseAccounts.Add(item);
                    totalExpense += balance;
                }
            }

            return new YearEndClosingPreview
            {
                FiscalYear = request.Request.FiscalYear,
                ClosingDate = endOfYear,
                RevenueAccounts = revenueAccounts,
                ExpenseAccounts = expenseAccounts,
                TotalRevenue = totalRevenue,
                TotalExpense = totalExpense,
                NetIncome = totalRevenue - totalExpense
            };
        }

        private async Task<decimal> GetAccountBalanceAsync(ChartOfAccountEntity account, DateTime endOfYear, CancellationToken cancellationToken)
        {
            var totalDebit = await _journalLineRepository.Query()
                .Include(jl => jl.JournalEntry)
                .Where(jl => jl.AccountId == account.AccountId
                    && jl.JournalEntry!.Status == "Posted"
                    && jl.JournalEntry.JournalDate <= endOfYear
                    && !jl.IsDeleted
                    && !jl.JournalEntry.IsDeleted)
                .SumAsync(jl => jl.DebitAmount, cancellationToken);

            var totalCredit = await _journalLineRepository.Query()
                .Include(jl => jl.JournalEntry)
                .Where(jl => jl.AccountId == account.AccountId
                    && jl.JournalEntry!.Status == "Posted"
                    && jl.JournalEntry.JournalDate <= endOfYear
                    && !jl.IsDeleted
                    && !jl.JournalEntry.IsDeleted)
                .SumAsync(jl => jl.CreditAmount, cancellationToken);

            return GeneralLedgerHelper.CalculateBalance(account.AccountType, totalDebit, totalCredit);
        }
    }
}