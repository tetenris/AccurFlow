using AccuFlow.Models.GoodsReceipt;
using MediatR;

namespace AccuFlow.Application.Features.GoodsReceipts.Commands
{
    public record UpdateGoodsReceiptCommand(UpdateGoodsReceiptRequest Request, Guid UserId) : IRequest;
}