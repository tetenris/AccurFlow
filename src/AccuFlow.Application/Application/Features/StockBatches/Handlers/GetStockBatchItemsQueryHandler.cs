using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockBatches.Queries;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockBatches.Handlers
{
    public class GetStockBatchItemsQueryHandler : IRequestHandler<GetStockBatchItemsQuery, List<ItemEntity>>
    {
        private readonly IRepository<ItemEntity> _itemRepository;

        public GetStockBatchItemsQueryHandler(IRepository<ItemEntity> itemRepository)
        {
            _itemRepository = itemRepository;
        }

        public async Task<List<ItemEntity>> Handle(GetStockBatchItemsQuery request, CancellationToken cancellationToken)
        {
            return await _itemRepository.Query()
                .Where(x => x.IsActive && !x.IsDeleted && x.ItemType == "Inventory")
                .OrderBy(x => x.ItemCode)
                .ToListAsync(cancellationToken);
        }
    }
}