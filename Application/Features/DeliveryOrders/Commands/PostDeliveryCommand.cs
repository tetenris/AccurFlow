using MediatR;

namespace AccuFlow.Application.Features.DeliveryOrders.Commands
{
    public record PostDeliveryCommand(Guid Id, Guid UserId) : IRequest;
}