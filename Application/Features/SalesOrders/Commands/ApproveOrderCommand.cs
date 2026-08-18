using MediatR;

namespace AccuFlow.Application.Features.SalesOrders.Commands
{
    public record ApproveOrderCommand(Guid Id, Guid UserId) : IRequest;
}