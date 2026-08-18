using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Production;
using MediatR;

namespace AccuFlow.Application.Features.ProductionOrders.Queries
{
    public record GetProductionOrderDatatableQuery(BaseDatatableRequest Request) : IRequest<BaseDatatableResponse>;
}