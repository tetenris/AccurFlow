using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.TrialBalances.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.TrialBalance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.TrialBalances.Handlers
{
    public class GetTrialBalanceQueryHandler : IRequestHandler<GetTrialBalanceQuery, TrialBalanceViewModel>
    {
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;
        private readonly IRepository<JournalLineEntity> _journalLineRepository;

        public GetTrialBalanceQueryHandler(
            IRepository<ChartOfAccountEntity> coaRepository,
            IRepository<JournalLineEntity> journalLineRepository)
        {
            _coaRepository = coaRepository;
            _journalLineRepository = journalLineRepository;
        }

        public async Task<TrialBalanceViewModel> Handle(GetTrialBalanceQuery request, CancellationToken cancellationToken)
        {
            var asOfDate = request.Request.AsOfDate ?? DateTime.Now;

            IQueryable<ChartOfAccountEntity> accountsQuery = _coaRepository.Query()
                .Where(a => !a.IsDeleted && a.IsActive && !a.IsHeader);

            if (!string.IsNullOrEmpty(request.Request.AccountType))
            {
                accountsQuery = accountsQuery.Where(a => a.AccountType == request.Request.AccountType);
            }

            var accounts = await accountsQuery.OrderBy(a => a.AccountCode).ToListAsync(cancellationToken);

            var lines = new List<TrialBalanceLineViewModel>();

            foreach (var account in accounts)
            {
                var balance = await GetAccountBalanceAsync(account, asOfDate, cancellationToken);

                if (!request.Request.ShowZeroBalance && balance == 0)
                    continue;

                var line = new TrialBalanceLineViewModel
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountCode,
                    AccountName = account.AccountName,
                    AccountType = account.AccountType
                };

                if (account.AccountType is "Asset" or "Expense" or "Other Expense")
                {
                    line.DebitBalance = balance >= 0 ? balance : 0;
                    line.CreditBalance = balance < 0 ? Math.Abs(balance) : 0;
                }
                else
                {
                    line.CreditBalance = balance >= 0 ? balance : 0;
                    line.DebitBalance = balance < 0 ? Math.Abs(balance) : 0;
                }

                lines.Add(line);
            }

            var groups = lines
                .GroupBy(l => l.AccountType)
                .OrderBy(g => GetAccountTypeOrder(g.Key))
                .Select(g => new TrialBalanceGroupViewModel
                {
                    AccountType = g.Key,
                    Accounts = g.OrderBy(a => a.AccountCode).ToList(),
                    SubtotalDebit = g.Sum(a => a.DebitBalance),
                    SubtotalCredit = g.Sum(a => a.CreditBalance)
                })
                .ToList();

            var totalDebit = groups.Sum(g => g.SubtotalDebit);
            var totalCredit = groups.Sum(g => g.SubtotalCredit);
            var difference = totalDebit - totalCredit;

            return new TrialBalanceViewModel
            {
                AsOfDate = asOfDate,
                Groups = groups,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                Difference = difference,
                IsBalanced = Math.Abs(difference) < 0.01m
            };
        }

        private async Task<decimal> GetAccountBalanceAsync(ChartOfAccountEntity account, DateTime asOfDate, CancellationToken cancellationToken)
        {
            var totalDebit = await _journalLineRepository.Query()
                .Include(jl => jl.JournalEntry)
                .Where(jl => jl.AccountId == account.AccountId
                    && jl.JournalEntry!.Status == "Posted"
                    && jl.JournalEntry.JournalDate <= asOfDate
                    && !jl.IsDeleted
                    && !jl.JournalEntry.IsDeleted)
                .SumAsync(jl => jl.DebitAmount, cancellationToken);

            var totalCredit = await _journalLineRepository.Query()
                .Include(jl => jl.JournalEntry)
                .Where(jl => jl.AccountId == account.AccountId
                    && jl.JournalEntry!.Status == "Posted"
                    && jl.JournalEntry.JournalDate <= asOfDate
                    && !jl.IsDeleted
                    && !jl.JournalEntry.IsDeleted)
                .SumAsync(jl => jl.CreditAmount, cancellationToken);

            return GeneralLedgerHelper.CalculateBalance(account.AccountType, totalDebit, totalCredit);
        }

        private int GetAccountTypeOrder(string accountType)
        {
            return accountType switch
            {
                "Asset" => 1,
                "Liability" => 2,
                "Equity" => 3,
                "Revenue" => 4,
                "Expense" => 5,
                "Other Income" => 6,
                "Other Expense" => 7,
                _ => 99
            };
        }
    }
}