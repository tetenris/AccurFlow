using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.JournalEntries.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.JournalEntries.Handlers
{
    public class ReverseJournalCommandHandler : IRequestHandler<ReverseJournalCommand>
    {
        private readonly IRepository<JournalEntryEntity> _journalRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReverseJournalCommandHandler(
            IRepository<JournalEntryEntity> journalRepository,
            IUnitOfWork unitOfWork)
        {
            _journalRepository = journalRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ReverseJournalCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var journal = await _journalRepository.Query()
                .Include(x => x.JournalLines.Where(l => !l.IsDeleted))
                    .ThenInclude(l => l.Account)
                .FirstOrDefaultAsync(x => x.JournalId == r.JournalId && !x.IsDeleted, cancellationToken);

            if (journal == null)
            {
                throw new Exception("Journal entry not found");
            }

            if (!journal.CanReverse)
            {
                throw new Exception("Cannot reverse this journal entry");
            }

            var reversalNumber = await JournalEntryHelper.GenerateJournalNumberAsync(
                _journalRepository, r.ReversalDate, journal.JournalType, cancellationToken);

            var reversalJournal = new JournalEntryEntity
            {
                JournalId = Guid.NewGuid(),
                JournalNumber = reversalNumber,
                JournalDate = r.ReversalDate,
                Description = $"Reversal of {journal.JournalNumber} - {journal.Description}",
                Status = "Posted",
                TotalDebit = journal.TotalCredit,
                TotalCredit = journal.TotalDebit,
                PostedDate = r.ReversalDate,
                PostedBy = userId,
                OriginalJournalId = journal.JournalId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            int lineNumber = 1;
            foreach (var line in journal.JournalLines.OrderBy(l => l.LineNumber))
            {
                reversalJournal.JournalLines.Add(new JournalLineEntity
                {
                    JournalLineId = Guid.NewGuid(),
                    LineNumber = lineNumber++,
                    AccountId = line.AccountId,
                    Description = line.Description,
                    DebitAmount = line.CreditAmount,
                    CreditAmount = line.DebitAmount,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            journal.Status = "Reversed";
            journal.ReversalJournalId = reversalJournal.JournalId;
            journal.UpdatedBy = userId;
            journal.UpdatedAt = DateTime.UtcNow;

            _journalRepository.Add(reversalJournal);
            _journalRepository.Update(journal);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}