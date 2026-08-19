using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.JournalEntries.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.JournalEntry;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.JournalEntries.Handlers
{
    public class GetJournalByIdQueryHandler : IRequestHandler<GetJournalByIdQuery, JournalEntryDetailViewModel?>
    {
        private readonly IRepository<JournalEntryEntity> _journalRepository;

        public GetJournalByIdQueryHandler(IRepository<JournalEntryEntity> journalRepository)
        {
            _journalRepository = journalRepository;
        }

        public async Task<JournalEntryDetailViewModel?> Handle(GetJournalByIdQuery request, CancellationToken cancellationToken)
        {
            var journal = await _journalRepository.Query()
                .Where(x => x.JournalId == request.JournalId && !x.IsDeleted)
                .Include(x => x.JournalLines.Where(l => !l.IsDeleted))
                    .ThenInclude(l => l.Account)
                .Include(x => x.CreatedByUser)
                .Include(x => x.UpdatedByUser)
                .Include(x => x.PostedByUser)
                .Include(x => x.ReversalJournal)
                .Include(x => x.OriginalJournal)
                .FirstOrDefaultAsync(cancellationToken);

            if (journal == null) return null;

            return new JournalEntryDetailViewModel
            {
                JournalId = journal.JournalId,
                JournalNumber = journal.JournalNumber,
                JournalDate = journal.JournalDate,
                Description = journal.Description,
                JournalType = journal.JournalType ?? "General",
                Status = journal.Status,
                TotalDebit = journal.TotalDebit,
                TotalCredit = journal.TotalCredit,
                IsBalanced = journal.IsBalanced,
                PostedDate = journal.PostedDate,
                PostedBy = journal.PostedByUser?.FullName,
                ReversalJournalNumber = journal.ReversalJournal?.JournalNumber,
                OriginalJournalNumber = journal.OriginalJournal?.JournalNumber,
                CreatedBy = journal.CreatedByUser?.FullName ?? "",
                CreatedAt = journal.CreatedAt,
                UpdatedBy = journal.UpdatedByUser?.FullName,
                UpdatedAt = journal.UpdatedAt,
                CanEdit = journal.CanEdit,
                CanDelete = journal.CanDelete,
                CanPost = journal.CanPost,
                CanReverse = journal.CanReverse,
                JournalLines = journal.JournalLines
                    .OrderBy(l => l.LineNumber)
                    .Select(l => new JournalLineViewModel
                    {
                        JournalLineId = l.JournalLineId,
                        LineNumber = l.LineNumber,
                        AccountId = l.AccountId,
                        AccountCode = l.Account.AccountCode,
                        AccountName = l.Account.AccountName,
                        Description = l.Description,
                        DebitAmount = l.DebitAmount,
                        CreditAmount = l.CreditAmount
                    })
                    .ToList()
            };
        }
    }
}