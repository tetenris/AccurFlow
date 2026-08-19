using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.FinancialStatements.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.FinancialStatement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.FinancialStatements.Handlers
{
    public class GetIncomeStatementQueryHandler : IRequestHandler<GetIncomeStatementQuery, IncomeStatementViewModel>
    {
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;
        private readonly IRepository<JournalLineEntity> _journalLineRepository;

        public GetIncomeStatementQueryHandler(
            IRepository<ChartOfAccountEntity> coaRepository,
            IRepository<JournalLineEntity> journalLineRepository)
        {
            _coaRepository = coaRepository;
            _journalLineRepository = journalLineRepository;
        }

        public async Task<IncomeStatementViewModel> Handle(GetIncomeStatementQuery request, CancellationToken cancellationToken)
        {
            var accounts = await _coaRepository.Query()
                .Where(a => !a.IsDeleted && a.IsActive && !a.IsHeader
                    && (a.AccountType == "Revenue" || a.AccountType == "Other Income"
                        || a.AccountType == "Expense" || a.AccountType == "Other Expense"))
                .OrderBy(a => a.AccountCode)
                .ToListAsync(cancellationToken);

            var sections = new List<IncomeStatementSectionViewModel>();
            var accountTypes = new[] { "Revenue", "Other Income", "Expense", "Other Expense" };

            foreach (var accountType in accountTypes)
            {
                var accountsInType = accounts.Where(a => a.AccountType == accountType).ToList();
                var lines = new List<IncomeStatementLineViewModel>();

                foreach (var account in accountsInType)
                {
                    var balance = await FinancialStatementHelper.GetAccountBalanceAsync(
                        _journalLineRepository, account.AccountId, request.Request.DateTo, cancellationToken);

                    if (!request.Request.ShowZeroBalance && balance == 0)
                        continue;

                    lines.Add(new IncomeStatementLineViewModel
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountCode,
                        AccountName = account.AccountName,
                        Amount = Math.Abs(balance)
                    });
                }

                if (lines.Any())
                {
                    sections.Add(new IncomeStatementSectionViewModel
                    {
                        SectionName = accountType,
                        Lines = lines,
                        Subtotal = lines.Sum(l => l.Amount)
                    });
                }
            }

            var totalRevenue = sections.Where(s => s.SectionName == "Revenue" || s.SectionName == "Other Income")
                .Sum(s => s.Subtotal);
            var totalExpense = sections.Where(s => s.SectionName == "Expense" || s.SectionName == "Other Expense")
                .Sum(s => s.Subtotal);

            return new IncomeStatementViewModel
            {
                DateFrom = request.Request.DateFrom,
                DateTo = request.Request.DateTo,
                Sections = sections,
                TotalRevenue = totalRevenue,
                TotalExpense = totalExpense,
                NetIncome = totalRevenue - totalExpense
            };
        }
    }
}