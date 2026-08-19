using AccuFlow.Models.StockTransfer;
using MediatR;

namespace AccuFlow.Application.Features.StockTransfers.Commands
{
    public record UpdateStockTransferCommand(UpdateStockTransferRequest Request, Guid UserId) : IRequest;
}