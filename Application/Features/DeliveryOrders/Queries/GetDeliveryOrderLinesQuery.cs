using AccuFlow.Models.Delivery;
using MediatR;

namespace AccuFlow.Application.Features.DeliveryOrders.Queries
{
    public record GetDeliveryOrderLinesQuery(Guid SalesOrderId) : IRequest<OrderDeliveryViewModel>;
}