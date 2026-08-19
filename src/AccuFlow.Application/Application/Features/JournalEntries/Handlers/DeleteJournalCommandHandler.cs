using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.JournalEntries.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.JournalEntries.Handlers
{
    public class DeleteJournalCommandHandler : IRequestHandler<DeleteJournalCommand>
    {
        private readonly IRepository<JournalEntryEntity> _journalRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteJournalCommandHandler(
            IRepository<JournalEntryEntity> journalRepository,
            IUnitOfWork unitOfWork)
        {
            _journalRepository = journalRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteJournalCommand request, CancellationToken cancellationToken)
        {
            var journal = await _journalRepository.Query()
                .Include(x => x.JournalLines)
                .FirstOrDefaultAsync(x => x.JournalId == request.JournalId && !x.IsDeleted, cancellationToken);

            if (journal == null)
            {
                throw new Exception("Journal entry not found");
            }

            if (!journal.CanDelete)
            {
                throw new Exception("Cannot delete this journal entry");
            }

            journal.IsDeleted = true;
            journal.DeletedBy = request.UserId;
            journal.DeletedAt = DateTime.UtcNow;

            foreach (var line in journal.JournalLines)
            {
                line.IsDeleted = true;
            }

            _journalRepository.Update(journal);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}