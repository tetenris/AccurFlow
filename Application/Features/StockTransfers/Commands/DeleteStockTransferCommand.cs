using MediatR;

namespace AccuFlow.Application.Features.StockTransfers.Commands
{
    public record DeleteStockTransferCommand(Guid Id, Guid UserId) : IRequest;
}