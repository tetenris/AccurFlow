using AccuFlow.Models.JournalEntry;
using MediatR;

namespace AccuFlow.Application.Features.JournalEntries.Commands
{
    public record ReverseJournalCommand(ReverseJournalRequest Request, Guid UserId) : IRequest;
}