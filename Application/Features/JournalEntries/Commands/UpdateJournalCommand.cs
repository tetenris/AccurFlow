using AccuFlow.Models.JournalEntry;
using MediatR;

namespace AccuFlow.Application.Features.JournalEntries.Commands
{
    public record UpdateJournalCommand(UpdateJournalEntryRequest Request, Guid UserId) : IRequest;
}