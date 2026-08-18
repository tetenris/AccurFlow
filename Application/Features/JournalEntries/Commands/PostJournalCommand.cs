using AccuFlow.Models.JournalEntry;
using MediatR;

namespace AccuFlow.Application.Features.JournalEntries.Commands
{
    public record PostJournalCommand(PostJournalRequest Request, Guid UserId) : IRequest;
}