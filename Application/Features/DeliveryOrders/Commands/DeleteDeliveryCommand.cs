using MediatR;

namespace AccuFlow.Application.Features.DeliveryOrders.Commands
{
    public record DeleteDeliveryCommand(Guid Id, Guid UserId) : IRequest;
}