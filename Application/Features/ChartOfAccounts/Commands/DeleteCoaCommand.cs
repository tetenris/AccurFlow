using MediatR;

namespace AccuFlow.Application.Features.ChartOfAccounts.Commands
{
    public record DeleteCoaCommand(Guid AccountId, Guid UserId) : IRequest;
}