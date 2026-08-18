using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.GeneralLedgers.Queries;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.GeneralLedgers.Handlers
{
    public class GetAccountBalanceQueryHandler : IRequestHandler<GetAccountBalanceQuery, decimal>
    {
        private readonly IRepository<JournalLineEntity> _journalLineRepository;
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;

        public GetAccountBalanceQueryHandler(
            IRepository<JournalLineEntity> journalLineRepository,
            IRepository<ChartOfAccountEntity> coaRepository)
        {
            _journalLineRepository = journalLineRepository;
            _coaRepository = coaRepository;
        }

        public async Task<decimal> Handle(GetAccountBalanceQuery request, CancellationToken cancellationToken)
        {
            var account = await _coaRepository.FirstOrDefaultAsync(
                a => a.AccountId == request.AccountId,
                cancellationToken);

            if (account == null)
                return 0;

            IQueryable<JournalLineEntity> query = _journalLineRepository.Query()
                .Include(jl => jl.JournalEntry)
                .Where(jl => jl.AccountId == request.AccountId
                    && jl.JournalEntry!.Status == "Posted"
                    && !jl.IsDeleted
                    && !jl.JournalEntry.IsDeleted);

            if (request.AsOfDate.HasValue)
            {
                query = query.Where(jl => jl.JournalEntry!.JournalDate <= request.AsOfDate.Value);
            }

            var totalDebit = await query.SumAsync(jl => jl.DebitAmount, cancellationToken);
            var totalCredit = await query.SumAsync(jl => jl.CreditAmount, cancellationToken);

            return GeneralLedgerHelper.CalculateBalance(account.AccountType, totalDebit, totalCredit);
        }
    }
}