using MediatR;

namespace AccuFlow.Application.Features.ChartOfAccounts.Commands
{
    public record ToggleCoaStatusCommand(Guid AccountId, Guid UserId) : IRequest;
}