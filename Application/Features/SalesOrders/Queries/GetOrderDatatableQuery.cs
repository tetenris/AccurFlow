using AccuFlow.Models.BaseModel;
using AccuFlow.Models.SalesOrder;
using MediatR;

namespace AccuFlow.Application.Features.SalesOrders.Queries
{
    public record GetOrderDatatableQuery(DataTableOrderRequest Request) : IRequest<BaseDatatableResponse>;
}