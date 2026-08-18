using AccuFlow.Models.SalesOrder;
using MediatR;

namespace AccuFlow.Application.Features.SalesOrders.Queries
{
    public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDetailViewModel?>;
}