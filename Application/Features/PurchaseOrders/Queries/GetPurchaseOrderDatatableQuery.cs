using AccuFlow.Models.BaseModel;
using AccuFlow.Models.PurchaseOrder;
using MediatR;

namespace AccuFlow.Application.Features.PurchaseOrders.Queries
{
    public record GetPurchaseOrderDatatableQuery(DataTablePurchaseOrderRequest Request) : IRequest<BaseDatatableResponse>;
}