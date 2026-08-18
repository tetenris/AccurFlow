using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Items.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Items.Handlers
{
    public class GetStockMinimumQueryHandler : IRequestHandler<GetStockMinimumQuery, BaseDatatableResponse>
    {
        private readonly IRepository<ItemEntity> _itemRepository;
        private readonly IRepository<StockMovementEntity> _stockMovementRepository;

        public GetStockMinimumQueryHandler(
            IRepository<ItemEntity> itemRepository,
            IRepository<StockMovementEntity> stockMovementRepository)
        {
            _itemRepository = itemRepository;
            _stockMovementRepository = stockMovementRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetStockMinimumQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var items = await _itemRepository.Query()
                .Where(x => !x.IsDeleted && x.ItemType == "Inventory")
                .Select(x => new { x.ItemId, x.ItemCode, x.ItemName, x.Unit, x.ReorderPoint })
                .ToListAsync(cancellationToken);
            var stock = await _stockMovementRepository.Query()
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.ItemId)
                .Select(g => new { ItemId = g.Key, Quantity = g.Sum(x => x.QuantityIn - x.QuantityOut) })
                .ToListAsync(cancellationToken);
            var stockMap = stock.ToDictionary(x => x.ItemId, x => x.Quantity);

            var rows = items.Select(x => new StockMinimumViewModel
            {
                ItemId = x.ItemId,
                ItemCode = x.ItemCode,
                ItemName = x.ItemName,
                Unit = x.Unit,
                ReorderPoint = x.ReorderPoint,
                CurrentStock = stockMap.ContainsKey(x.ItemId) ? stockMap[x.ItemId] : 0,
                IsBelow = (stockMap.ContainsKey(x.ItemId) ? stockMap[x.ItemId] : 0) < x.ReorderPoint
            }).ToList();

            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                rows = rows.Where(x => x.ItemCode.ToLower().Contains(search) || x.ItemName.ToLower().Contains(search)).ToList();
            }
            if (r.BelowOnly) rows = rows.Where(x => x.IsBelow).ToList();
            var total = rows.Count;
            var data = rows.OrderByDescending(x => x.IsBelow).ThenBy(x => x.ItemCode).Skip((r.Page - 1) * r.Size).Take(r.Size).ToList();
            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}