using AccuFlow.Models.JournalEntry;
using MediatR;

namespace AccuFlow.Application.Features.JournalEntries.Queries
{
    public record GetJournalByIdQuery(Guid JournalId) : IRequest<JournalEntryDetailViewModel?>;
}