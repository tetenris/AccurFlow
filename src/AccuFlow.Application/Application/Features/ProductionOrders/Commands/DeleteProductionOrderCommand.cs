using MediatR;

namespace AccuFlow.Application.Features.ProductionOrders.Commands
{
    public record DeleteProductionOrderCommand(Guid ProductionOrderId, Guid UserId) : IRequest;
}