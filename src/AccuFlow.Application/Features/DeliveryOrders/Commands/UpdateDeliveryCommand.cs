using AccuFlow.Models.Delivery;
using MediatR;

namespace AccuFlow.Application.Features.DeliveryOrders.Commands
{
    public record UpdateDeliveryCommand(UpdateDeliveryRequest Request, Guid UserId) : IRequest;
}