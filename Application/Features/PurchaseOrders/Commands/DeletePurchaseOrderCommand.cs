using MediatR;

namespace AccuFlow.Application.Features.PurchaseOrders.Commands
{
    public record DeletePurchaseOrderCommand(Guid Id, Guid UserId) : IRequest;
}