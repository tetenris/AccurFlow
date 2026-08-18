using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockTransfers.Queries;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockTransfers.Handlers
{
    public class GetStockTransferWarehousesQueryHandler : IRequestHandler<GetStockTransferWarehousesQuery, List<WarehouseEntity>>
    {
        private readonly IRepository<WarehouseEntity> _warehouseRepository;

        public GetStockTransferWarehousesQueryHandler(IRepository<WarehouseEntity> warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<List<WarehouseEntity>> Handle(GetStockTransferWarehousesQuery request, CancellationToken cancellationToken)
        {
            return await _warehouseRepository.Query()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.WarehouseCode)
                .ToListAsync(cancellationToken);
        }
    }
}