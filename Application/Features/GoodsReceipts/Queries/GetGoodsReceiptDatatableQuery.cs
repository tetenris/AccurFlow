using AccuFlow.Models.BaseModel;
using AccuFlow.Models.GoodsReceipt;
using MediatR;

namespace AccuFlow.Application.Features.GoodsReceipts.Queries
{
    public record GetGoodsReceiptDatatableQuery(DataTableGoodsReceiptRequest Request) : IRequest<BaseDatatableResponse>;
}