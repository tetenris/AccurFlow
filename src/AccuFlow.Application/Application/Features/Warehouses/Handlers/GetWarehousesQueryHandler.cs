using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Warehouses.Queries;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Warehouses.Handlers
{
    public class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, List<WarehouseEntity>>
    {
        private readonly IRepository<WarehouseEntity> _warehouseRepository;

        public GetWarehousesQueryHandler(IRepository<WarehouseEntity> warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<List<WarehouseEntity>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
        {
            return await _warehouseRepository.Query()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.WarehouseCode)
                .ToListAsync(cancellationToken);
        }
    }
}