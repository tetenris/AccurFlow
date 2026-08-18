using AccuFlow.Models.GoodsReceipt;
using MediatR;

namespace AccuFlow.Application.Features.GoodsReceipts.Queries
{
    public record GetGoodsReceiptByIdQuery(Guid Id) : IRequest<GoodsReceiptDetailViewModel?>;
}