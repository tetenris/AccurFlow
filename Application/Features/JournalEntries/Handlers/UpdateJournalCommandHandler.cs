using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.JournalEntries.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.JournalEntry;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.JournalEntries.Handlers
{
    public class UpdateJournalCommandHandler : IRequestHandler<UpdateJournalCommand>
    {
        private readonly IRepository<JournalEntryEntity> _journalRepository;
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateJournalCommandHandler(
            IRepository<JournalEntryEntity> journalRepository,
            IRepository<ChartOfAccountEntity> coaRepository,
            IUnitOfWork unitOfWork)
        {
            _journalRepository = journalRepository;
            _coaRepository = coaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateJournalCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var journal = await _journalRepository.Query()
                .Include(x => x.JournalLines)
                .FirstOrDefaultAsync(x => x.JournalId == r.JournalId && !x.IsDeleted, cancellationToken);

            if (journal == null)
            {
                throw new Exception("Journal entry not found");
            }

            if (!journal.CanEdit)
            {
                throw new Exception("Cannot edit this journal entry");
            }

            var errors = await JournalEntryHelper.ValidateAsync(_coaRepository, new CreateJournalEntryRequest
            {
                JournalDate = r.JournalDate,
                Description = r.Description,
                JournalLines = r.JournalLines
            }, cancellationToken);

            if (errors.Any())
            {
                throw new Exception(string.Join(", ", errors));
            }

            journal.JournalDate = r.JournalDate;
            journal.Description = r.Description;
            journal.TotalDebit = r.JournalLines.Sum(x => x.DebitAmount);
            journal.TotalCredit = r.JournalLines.Sum(x => x.CreditAmount);
            journal.UpdatedBy = userId;
            journal.UpdatedAt = DateTime.UtcNow;

            foreach (var line in journal.JournalLines)
            {
                line.IsDeleted = true;
            }

            int lineNumber = 1;
            foreach (var line in r.JournalLines)
            {
                journal.JournalLines.Add(new JournalLineEntity
                {
                    JournalLineId = Guid.NewGuid(),
                    LineNumber = lineNumber++,
                    AccountId = line.AccountId,
                    Description = line.Description,
                    DebitAmount = line.DebitAmount,
                    CreditAmount = line.CreditAmount,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _journalRepository.Update(journal);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}