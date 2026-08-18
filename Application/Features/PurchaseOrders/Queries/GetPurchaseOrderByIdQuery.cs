using AccuFlow.Models.PurchaseOrder;
using MediatR;

namespace AccuFlow.Application.Features.PurchaseOrders.Queries
{
    public record GetPurchaseOrderByIdQuery(Guid Id) : IRequest<PurchaseOrderDetailViewModel?>;
}