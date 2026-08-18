using AccuFlow.Models.Delivery;
using MediatR;

namespace AccuFlow.Application.Features.DeliveryOrders.Commands
{
    public record CreateDeliveryCommand(CreateDeliveryRequest Request, Guid UserId) : IRequest;
}