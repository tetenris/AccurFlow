using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockOpnames.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.StockOpname;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockOpnames.Handlers
{
    public class GetStockOpnameQuantitiesQueryHandler : IRequestHandler<GetStockOpnameQuantitiesQuery, List<StockOpnameLineViewModel>>
    {
        private readonly IRepository<StockMovementEntity> _stockMovementRepository;
        private readonly IRepository<ItemEntity> _itemRepository;

        public GetStockOpnameQuantitiesQueryHandler(
            IRepository<StockMovementEntity> stockMovementRepository,
            IRepository<ItemEntity> itemRepository)
        {
            _stockMovementRepository = stockMovementRepository;
            _itemRepository = itemRepository;
        }

        public async Task<List<StockOpnameLineViewModel>> Handle(GetStockOpnameQuantitiesQuery request, CancellationToken cancellationToken)
        {
            var current = await GetCurrentStockAsync(request.WarehouseId, cancellationToken);
            var items = await _itemRepository.Query()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.ItemCode)
                .ToListAsync(cancellationToken);

            return items
                .Where(i => current.ContainsKey(i.ItemId))
                .Select(i => new StockOpnameLineViewModel
                {
                    ItemId = i.ItemId,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    Unit = i.Unit,
                    SystemQuantity = current[i.ItemId],
                    ActualQuantity = current[i.ItemId],
                    DifferenceQuantity = 0,
                    UnitCost = i.PurchasePrice
                })
                .ToList();
        }

        private async Task<Dictionary<Guid, decimal>> GetCurrentStockAsync(Guid warehouseId, CancellationToken cancellationToken)
        {
            return await _stockMovementRepository.Query()
                .Where(x => x.WarehouseId == warehouseId && !x.IsDeleted)
                .GroupBy(x => x.ItemId)
                .Select(g => new { ItemId = g.Key, Quantity = g.Sum(m => m.QuantityIn - m.QuantityOut) })
                .ToDictionaryAsync(x => x.ItemId, x => x.Quantity, cancellationToken);
        }
    }
}