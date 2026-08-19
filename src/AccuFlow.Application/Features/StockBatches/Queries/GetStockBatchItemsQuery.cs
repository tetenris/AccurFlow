using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.StockBatches.Queries
{
    public record GetStockBatchItemsQuery() : IRequest<List<ItemEntity>>;
}