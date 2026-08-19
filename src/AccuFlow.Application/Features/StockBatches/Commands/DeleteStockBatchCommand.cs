using MediatR;

namespace AccuFlow.Application.Features.StockBatches.Commands
{
    public record DeleteStockBatchCommand(Guid Id, Guid UserId) : IRequest;
}