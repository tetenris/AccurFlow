using MediatR;

namespace AccuFlow.Application.Features.BankReconciliations.Commands
{
    public record DeleteReconciliationCommand(Guid ReconciliationId, Guid UserId) : IRequest;
}