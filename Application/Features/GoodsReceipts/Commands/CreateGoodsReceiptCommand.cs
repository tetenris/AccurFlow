using AccuFlow.Models.GoodsReceipt;
using MediatR;

namespace AccuFlow.Application.Features.GoodsReceipts.Commands
{
    public record CreateGoodsReceiptCommand(CreateGoodsReceiptRequest Request, Guid UserId) : IRequest;
}