using MediatR;

namespace AccuFlow.Application.Features.PurchaseOrders.Commands
{
    public record ApprovePurchaseOrderCommand(Guid Id, Guid UserId) : IRequest;
}