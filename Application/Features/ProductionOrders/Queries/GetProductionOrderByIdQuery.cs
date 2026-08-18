using AccuFlow.Models.Production;
using MediatR;

namespace AccuFlow.Application.Features.ProductionOrders.Queries
{
    public record GetProductionOrderByIdQuery(Guid ProductionOrderId) : IRequest<ProductionOrderDetailViewModel?>;
}