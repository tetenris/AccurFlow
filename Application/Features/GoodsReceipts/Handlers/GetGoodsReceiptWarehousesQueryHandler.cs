using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.GoodsReceipts.Queries;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.GoodsReceipts.Handlers
{
    public class GetGoodsReceiptWarehousesQueryHandler : IRequestHandler<GetGoodsReceiptWarehousesQuery, List<WarehouseEntity>>
    {
        private readonly IRepository<WarehouseEntity> _warehouseRepository;

        public GetGoodsReceiptWarehousesQueryHandler(IRepository<WarehouseEntity> warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<List<WarehouseEntity>> Handle(GetGoodsReceiptWarehousesQuery request, CancellationToken cancellationToken)
        {
            return await _warehouseRepository.Query()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.WarehouseCode)
                .ToListAsync(cancellationToken);
        }
    }
}