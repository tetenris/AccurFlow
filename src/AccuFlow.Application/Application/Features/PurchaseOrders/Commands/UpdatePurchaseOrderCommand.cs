using AccuFlow.Models.PurchaseOrder;
using MediatR;

namespace AccuFlow.Application.Features.PurchaseOrders.Commands
{
    public record UpdatePurchaseOrderCommand(UpdatePurchaseOrderRequest Request, Guid UserId) : IRequest;
}