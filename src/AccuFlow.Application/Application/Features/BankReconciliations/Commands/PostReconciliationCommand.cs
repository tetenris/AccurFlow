using MediatR;

namespace AccuFlow.Application.Features.BankReconciliations.Commands
{
    public record PostReconciliationCommand(Guid ReconciliationId, Guid UserId) : IRequest;
}