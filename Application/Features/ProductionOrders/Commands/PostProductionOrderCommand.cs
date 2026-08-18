using MediatR;

namespace AccuFlow.Application.Features.ProductionOrders.Commands
{
    public record PostProductionOrderCommand(Guid ProductionOrderId, Guid UserId) : IRequest;
}