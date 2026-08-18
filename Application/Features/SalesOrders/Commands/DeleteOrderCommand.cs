using MediatR;

namespace AccuFlow.Application.Features.SalesOrders.Commands
{
    public record DeleteOrderCommand(Guid Id, Guid UserId) : IRequest;
}