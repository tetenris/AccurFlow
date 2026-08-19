using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.StockTransfers.Queries
{
    public record GetStockTransferWarehousesQuery() : IRequest<List<WarehouseEntity>>;
}