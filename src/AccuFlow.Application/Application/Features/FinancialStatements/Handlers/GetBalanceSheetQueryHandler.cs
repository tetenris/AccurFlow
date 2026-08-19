using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.FinancialStatements.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.FinancialStatement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.FinancialStatements.Handlers
{
    public class GetBalanceSheetQueryHandler : IRequestHandler<GetBalanceSheetQuery, BalanceSheetViewModel>
    {
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;
        private readonly IRepository<JournalLineEntity> _journalLineRepository;

        public GetBalanceSheetQueryHandler(
            IRepository<ChartOfAccountEntity> coaRepository,
            IRepository<JournalLineEntity> journalLineRepository)
        {
            _coaRepository = coaRepository;
            _journalLineRepository = journalLineRepository;
        }

        public async Task<BalanceSheetViewModel> Handle(GetBalanceSheetQuery request, CancellationToken cancellationToken)
        {
            var accounts = await _coaRepository.Query()
                .Where(a => !a.IsDeleted && a.IsActive && !a.IsHeader
                    && (a.AccountType == "Asset" || a.AccountType == "Liability" || a.AccountType == "Equity"))
                .OrderBy(a => a.AccountCode)
                .ToListAsync(cancellationToken);

            var sections = new List<BalanceSheetSectionViewModel>();
            var accountTypes = new[] { "Asset", "Liability", "Equity" };

            foreach (var accountType in accountTypes)
            {
                var accountsInType = accounts.Where(a => a.AccountType == accountType).ToList();
                var lines = new List<BalanceSheetLineViewModel>();

                foreach (var account in accountsInType)
                {
                    var balance = await FinancialStatementHelper.GetAccountBalanceAsync(
                        _journalLineRepository, account.AccountId, request.Request.AsOfDate, cancellationToken);

                    if (!request.Request.ShowZeroBalance && balance == 0)
                        continue;

                    lines.Add(new BalanceSheetLineViewModel
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountCode,
                        AccountName = account.AccountName,
                        Amount = Math.Abs(balance)
                    });
                }

                if (lines.Any())
                {
                    sections.Add(new BalanceSheetSectionViewModel
                    {
                        SectionName = accountType,
                        Lines = lines,
                        Subtotal = lines.Sum(l => l.Amount)
                    });
                }
            }

            var totalAssets = sections.Where(s => s.SectionName == "Asset").Sum(s => s.Subtotal);
            var totalLiabilities = sections.Where(s => s.SectionName == "Liability").Sum(s => s.Subtotal);
            var totalEquity = sections.Where(s => s.SectionName == "Equity").Sum(s => s.Subtotal);
            var difference = totalAssets - (totalLiabilities + totalEquity);

            return new BalanceSheetViewModel
            {
                AsOfDate = request.Request.AsOfDate,
                Sections = sections,
                TotalAssets = totalAssets,
                TotalLiabilities = totalLiabilities,
                TotalEquity = totalEquity,
                IsBalanced = Math.Abs(difference) < 0.01m,
                Difference = difference
            };
        }
    }
}