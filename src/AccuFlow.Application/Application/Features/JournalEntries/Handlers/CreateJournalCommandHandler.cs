using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.JournalEntries.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.JournalEntries.Handlers
{
    public class CreateJournalCommandHandler : IRequestHandler<CreateJournalCommand, Guid>
    {
        private readonly IRepository<JournalEntryEntity> _journalRepository;
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateJournalCommandHandler(
            IRepository<JournalEntryEntity> journalRepository,
            IRepository<ChartOfAccountEntity> coaRepository,
            IUnitOfWork unitOfWork)
        {
            _journalRepository = journalRepository;
            _coaRepository = coaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateJournalCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var errors = await JournalEntryHelper.ValidateAsync(_coaRepository, r, cancellationToken);
            if (errors.Any())
            {
                throw new Exception(string.Join(", ", errors));
            }

            var journalNumber = await JournalEntryHelper.GenerateJournalNumberAsync(
                _journalRepository, r.JournalDate, r.JournalType, cancellationToken);

            var totalDebit = r.JournalLines.Sum(x => x.DebitAmount);
            var totalCredit = r.JournalLines.Sum(x => x.CreditAmount);

            var journal = new JournalEntryEntity
            {
                JournalId = Guid.NewGuid(),
                JournalNumber = journalNumber,
                JournalDate = r.JournalDate,
                Description = r.Description,
                JournalType = string.IsNullOrEmpty(r.JournalType) ? "General" : r.JournalType,
                Status = "Draft",
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

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

            _journalRepository.Add(journal);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return journal.JournalId;
        }
    }
}