using AccuFlow.Models.PurchaseOrder;
using MediatR;

namespace AccuFlow.Application.Features.PurchaseOrders.Commands
{
    public record CreatePurchaseOrderCommand(CreatePurchaseOrderRequest Request, Guid UserId) : IRequest;
}