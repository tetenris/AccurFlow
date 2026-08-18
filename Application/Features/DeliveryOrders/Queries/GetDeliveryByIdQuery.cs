using AccuFlow.Models.Delivery;
using MediatR;

namespace AccuFlow.Application.Features.DeliveryOrders.Queries
{
    public record GetDeliveryByIdQuery(Guid Id) : IRequest<DeliveryDetailViewModel?>;
}