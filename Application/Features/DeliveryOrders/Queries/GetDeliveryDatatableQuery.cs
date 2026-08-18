using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Delivery;
using MediatR;

namespace AccuFlow.Application.Features.DeliveryOrders.Queries
{
    public record GetDeliveryDatatableQuery(DataTableDeliveryRequest Request) : IRequest<BaseDatatableResponse>;
}