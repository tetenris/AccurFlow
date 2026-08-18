using AccuFlow.Models.StockBatch;
using MediatR;

namespace AccuFlow.Application.Features.StockBatches.Commands
{
    public record ConsumeStockBatchCommand(StockBatchConsumeRequest Request, Guid UserId) : IRequest;
}