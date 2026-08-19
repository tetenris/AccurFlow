using AccuFlow.Models.JournalEntry;
using MediatR;

namespace AccuFlow.Application.Features.JournalEntries.Commands
{
    public record CreateJournalCommand(CreateJournalEntryRequest Request, Guid UserId) : IRequest<Guid>;
}