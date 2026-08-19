using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockOpnames.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.StockOpname;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockOpnames.Handlers
{
    public class GetStockOpnameWarehousesQueryHandler : IRequestHandler<GetStockOpnameWarehousesQuery, List<WarehouseViewModel>>
    {
        private readonly IRepository<WarehouseEntity> _warehouseRepository;

        public GetStockOpnameWarehousesQueryHandler(IRepository<WarehouseEntity> warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<List<WarehouseViewModel>> Handle(GetStockOpnameWarehousesQuery request, CancellationToken cancellationToken)
        {
            return await _warehouseRepository.Query()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.WarehouseCode)
                .Select(x => new WarehouseViewModel
                {
                    WarehouseId = x.WarehouseId,
                    WarehouseCode = x.WarehouseCode,
                    WarehouseName = x.WarehouseName
                })
                .ToListAsync(cancellationToken);
        }
    }
}