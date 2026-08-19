using AccuFlow.Models.Production;
using MediatR;

namespace AccuFlow.Application.Features.ProductionOrders.Commands
{
    public record CreateProductionOrderCommand(CreateProductionOrderRequest Request, Guid UserId) : IRequest;
}