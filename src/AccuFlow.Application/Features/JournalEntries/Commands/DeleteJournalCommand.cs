using MediatR;

namespace AccuFlow.Application.Features.JournalEntries.Commands
{
    public record DeleteJournalCommand(Guid JournalId, Guid UserId) : IRequest;
}