using MediatR;

namespace AccuFlow.Application.Features.GoodsReceipts.Commands
{
    public record PostGoodsReceiptCommand(Guid Id, Guid UserId) : IRequest;
}