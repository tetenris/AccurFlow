using AccuFlow.Models.BaseModel;
using MediatR;

namespace AccuFlow.Application.Features.StockBatches.Queries
{
    public record GetStockBatchDatatableQuery(BaseDatatableRequest Request, Guid? ItemId) : IRequest<BaseDatatableResponse>;
}