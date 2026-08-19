using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.BankReconciliations.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.CashBank;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.BankReconciliations.Handlers
{
    public class GetBankStatementQueryHandler : IRequestHandler<GetBankStatementQuery, List<ReconciliationLineViewModel>>
    {
        private readonly IRepository<JournalLineEntity> _journalLineRepository;

        public GetBankStatementQueryHandler(IRepository<JournalLineEntity> journalLineRepository)
        {
            _journalLineRepository = journalLineRepository;
        }

        public async Task<List<ReconciliationLineViewModel>> Handle(GetBankStatementQuery request, CancellationToken cancellationToken)
        {
            var endDate = request.AsOfDate.Date;
            return await _journalLineRepository.Query()
                .Include(x => x.JournalEntry)
                .Where(x => x.AccountId == request.AccountId && !x.IsDeleted
                    && !x.JournalEntry.IsDeleted
                    && x.JournalEntry.Status == "Posted"
                    && x.JournalEntry.JournalDate <= endDate)
                .OrderBy(x => x.JournalEntry.JournalDate)
                .Select(x => new ReconciliationLineViewModel
                {
                    TransactionDate = x.JournalEntry.JournalDate,
                    DocumentNumber = x.JournalEntry.JournalNumber,
                    Description = x.JournalEntry.Description,
                    Amount = x.DebitAmount - x.CreditAmount,
                    IsCleared = true
                }).ToListAsync(cancellationToken);
        }
    }
}