using AccuFlow.Models.StockTransfer;
using MediatR;

namespace AccuFlow.Application.Features.StockTransfers.Commands
{
    public record CreateStockTransferCommand(CreateStockTransferRequest Request, Guid UserId) : IRequest;
}