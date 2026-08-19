using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.JournalEntries.Queries;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.JournalEntries.Handlers
{
    public class GenerateJournalNumberQueryHandler : IRequestHandler<GenerateJournalNumberQuery, string>
    {
        private readonly IRepository<JournalEntryEntity> _journalRepository;

        public GenerateJournalNumberQueryHandler(IRepository<JournalEntryEntity> journalRepository)
        {
            _journalRepository = journalRepository;
        }

        public Task<string> Handle(GenerateJournalNumberQuery request, CancellationToken cancellationToken)
        {
            return JournalEntryHelper.GenerateJournalNumberAsync(
                _journalRepository, request.JournalDate, request.JournalType, cancellationToken);
        }
    }
}