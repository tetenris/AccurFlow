using AccuFlow.Models.StockBatch;
using MediatR;

namespace AccuFlow.Application.Features.StockBatches.Commands
{
    public record CreateStockBatchCommand(StockBatchCreateRequest Request, Guid UserId) : IRequest;
}