using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.GeneralLedgers.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.GeneralLedger;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.GeneralLedgers.Handlers
{
    public class GetLedgerSummaryQueryHandler : IRequestHandler<GetLedgerSummaryQuery, List<LedgerSummaryViewModel>>
    {
        private readonly IRepository<JournalLineEntity> _journalLineRepository;

        public GetLedgerSummaryQueryHandler(IRepository<JournalLineEntity> journalLineRepository)
        {
            _journalLineRepository = journalLineRepository;
        }

        public async Task<List<LedgerSummaryViewModel>> Handle(GetLedgerSummaryQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;

            IQueryable<JournalLineEntity> query = _journalLineRepository.Query()
                .Include(jl => jl.JournalEntry)
                .Include(jl => jl.Account)
                .Where(jl => (jl.JournalEntry!.Status == "Posted" || jl.JournalEntry.Status == "Reversed")
                    && !jl.IsDeleted
                    && !jl.JournalEntry.IsDeleted);

            if (r.DateFrom.HasValue)
            {
                query = query.Where(jl => jl.JournalEntry!.JournalDate >= r.DateFrom.Value);
            }

            if (r.DateTo.HasValue)
            {
                query = query.Where(jl => jl.JournalEntry!.JournalDate <= r.DateTo.Value);
            }

            if (!string.IsNullOrEmpty(r.AccountType))
            {
                query = query.Where(jl => jl.Account!.AccountType == r.AccountType);
            }

            if (!string.IsNullOrEmpty(r.Search))
            {
                query = query.Where(jl => jl.Account!.AccountCode.Contains(r.Search)
                    || jl.Account!.AccountName.Contains(r.Search));
            }

            var grouped = await query
                .GroupBy(jl => new
                {
                    jl.AccountId,
                    jl.Account!.AccountCode,
                    jl.Account.AccountName,
                    jl.Account.AccountType
                })
                .Select(g => new
                {
                    g.Key.AccountId,
                    g.Key.AccountCode,
                    g.Key.AccountName,
                    g.Key.AccountType,
                    TotalDebit = g.Sum(jl => jl.DebitAmount),
                    TotalCredit = g.Sum(jl => jl.CreditAmount),
                    TransactionCount = g.Count()
                })
                .ToListAsync(cancellationToken);

            return grouped.Select(g => new LedgerSummaryViewModel
            {
                AccountId = g.AccountId,
                AccountCode = g.AccountCode,
                AccountName = g.AccountName,
                AccountType = g.AccountType,
                TotalDebit = g.TotalDebit,
                TotalCredit = g.TotalCredit,
                Balance = GeneralLedgerHelper.CalculateBalance(g.AccountType, g.TotalDebit, g.TotalCredit),
                TransactionCount = g.TransactionCount
            }).ToList();
        }
    }
}