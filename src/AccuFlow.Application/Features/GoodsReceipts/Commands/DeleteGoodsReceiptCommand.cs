using MediatR;

namespace AccuFlow.Application.Features.GoodsReceipts.Commands
{
    public record DeleteGoodsReceiptCommand(Guid Id, Guid UserId) : IRequest;
}