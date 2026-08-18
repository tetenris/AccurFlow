using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.GoodsReceipts.Queries
{
    public record GetGoodsReceiptWarehousesQuery : IRequest<List<WarehouseEntity>>;
}