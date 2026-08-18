using MediatR;

namespace AccuFlow.Application.Features.PurchaseOrders.Commands
{
    public record ConvertPurchaseOrderToInvoiceCommand(Guid Id, Guid UserId) : IRequest;
}