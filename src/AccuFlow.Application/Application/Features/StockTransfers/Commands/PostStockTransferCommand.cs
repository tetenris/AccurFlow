using MediatR;

namespace AccuFlow.Application.Features.StockTransfers.Commands
{
    public record PostStockTransferCommand(Guid Id, Guid UserId) : IRequest;
}