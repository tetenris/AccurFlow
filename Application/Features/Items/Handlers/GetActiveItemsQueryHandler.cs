using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Items.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Items.Handlers
{
    public class GetActiveItemsQueryHandler : IRequestHandler<GetActiveItemsQuery, List<ItemViewModel>>
    {
        private readonly IRepository<ItemEntity> _itemRepository;

        public GetActiveItemsQueryHandler(IRepository<ItemEntity> itemRepository)
        {
            _itemRepository = itemRepository;
        }

        public async Task<List<ItemViewModel>> Handle(GetActiveItemsQuery request, CancellationToken cancellationToken)
        {
            return await _itemRepository.Query()
                .Include(x => x.ItemGroup)
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.ItemCode)
                .Select(x => new ItemViewModel
                {
                    ItemId = x.ItemId,
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName,
                    ItemType = x.ItemType,
                    ItemGroupId = x.ItemGroupId,
                    ItemGroupName = x.ItemGroup != null ? x.ItemGroup.GroupName : null,
                    Unit = x.Unit,
                    SalesPrice = x.SalesPrice,
                    PurchasePrice = x.PurchasePrice,
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);
        }
    }
}