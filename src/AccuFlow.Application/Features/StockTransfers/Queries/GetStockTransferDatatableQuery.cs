using AccuFlow.Models.BaseModel;
using AccuFlow.Models.StockTransfer;
using MediatR;

namespace AccuFlow.Application.Features.StockTransfers.Queries
{
    public record GetStockTransferDatatableQuery(DataTableStockTransferRequest Request) : IRequest<BaseDatatableResponse>;
}