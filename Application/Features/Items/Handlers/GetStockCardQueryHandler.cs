using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Items.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Items.Handlers
{
    public class GetStockCardQueryHandler : IRequestHandler<GetStockCardQuery, BaseDatatableResponse>
    {
        private readonly IRepository<StockMovementEntity> _stockMovementRepository;

        public GetStockCardQueryHandler(IRepository<StockMovementEntity> stockMovementRepository)
        {
            _stockMovementRepository = stockMovementRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetStockCardQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _stockMovementRepository.Query().Include(x => x.Item).Include(x => x.Warehouse).Where(x => !x.IsDeleted);
            if (r.ItemId.HasValue) query = query.Where(x => x.ItemId == r.ItemId);
            if (r.WarehouseId.HasValue) query = query.Where(x => x.WarehouseId == r.WarehouseId);
            if (r.DateFrom.HasValue) query = query.Where(x => x.MovementDate >= r.DateFrom.Value.Date);
            if (r.DateTo.HasValue) query = query.Where(x => x.MovementDate <= r.DateTo.Value.Date);

            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderBy(x => x.MovementDate).Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new StockMovementViewModel
                {
                    StockMovementId = x.StockMovementId,
                    MovementDate = x.MovementDate,
                    ItemName = x.Item.ItemName,
                    WarehouseName = x.Warehouse.WarehouseName,
                    MovementType = x.MovementType,
                    QuantityIn = x.QuantityIn,
                    QuantityOut = x.QuantityOut,
                    UnitCost = x.UnitCost
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}