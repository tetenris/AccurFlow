using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.JournalEntries.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.JournalEntries.Handlers
{
    public class PostJournalCommandHandler : IRequestHandler<PostJournalCommand>
    {
        private readonly IRepository<JournalEntryEntity> _journalRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PostJournalCommandHandler(
            IRepository<JournalEntryEntity> journalRepository,
            IUnitOfWork unitOfWork)
        {
            _journalRepository = journalRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(PostJournalCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;

            var journal = await _journalRepository.Query()
                .Include(x => x.JournalLines)
                    .ThenInclude(l => l.Account)
                .FirstOrDefaultAsync(x => x.JournalId == r.JournalId && !x.IsDeleted, cancellationToken);

            if (journal == null)
            {
                throw new Exception("Journal entry not found");
            }

            if (!journal.CanPost)
            {
                throw new Exception("Cannot post this journal entry");
            }

            foreach (var line in journal.JournalLines.Where(l => !l.IsDeleted))
            {
                if (line.Account == null || !line.Account.IsActive)
                {
                    throw new Exception($"Account {line.Account?.AccountCode} is not active");
                }
            }

            journal.Status = "Posted";
            journal.PostedDate = r.PostedDate;
            journal.PostedBy = request.UserId;
            journal.UpdatedBy = request.UserId;
            journal.UpdatedAt = DateTime.UtcNow;

            _journalRepository.Update(journal);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}