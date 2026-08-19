using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.JournalEntries.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.JournalEntry;
using MediatR;

namespace AccuFlow.Application.Features.JournalEntries.Handlers
{
    public class GetJournalByNumberQueryHandler : IRequestHandler<GetJournalByNumberQuery, JournalEntryDetailViewModel?>
    {
        private readonly IRepository<JournalEntryEntity> _journalRepository;
        private readonly GetJournalByIdQueryHandler _getByIdHandler;

        public GetJournalByNumberQueryHandler(IRepository<JournalEntryEntity> journalRepository)
        {
            _journalRepository = journalRepository;
            _getByIdHandler = new GetJournalByIdQueryHandler(journalRepository);
        }

        public async Task<JournalEntryDetailViewModel?> Handle(GetJournalByNumberQuery request, CancellationToken cancellationToken)
        {
            var journal = await _journalRepository.FirstOrDefaultAsync(
                x => x.JournalNumber == request.JournalNumber && !x.IsDeleted,
                cancellationToken);

            if (journal == null) return null;

            return await _getByIdHandler.Handle(new GetJournalByIdQuery(journal.JournalId), cancellationToken);
        }
    }
}