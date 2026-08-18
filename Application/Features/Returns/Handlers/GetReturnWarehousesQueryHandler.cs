using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Returns.Queries;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Returns.Handlers
{
    public class GetReturnWarehousesQueryHandler : IRequestHandler<GetReturnWarehousesQuery, List<WarehouseEntity>>
    {
        private readonly IRepository<WarehouseEntity> _warehouseRepository;

        public GetReturnWarehousesQueryHandler(IRepository<WarehouseEntity> warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<List<WarehouseEntity>> Handle(GetReturnWarehousesQuery request, CancellationToken cancellationToken)
        {
            return await _warehouseRepository.Query()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.WarehouseCode)
                .ToListAsync(cancellationToken);
        }
    }
}