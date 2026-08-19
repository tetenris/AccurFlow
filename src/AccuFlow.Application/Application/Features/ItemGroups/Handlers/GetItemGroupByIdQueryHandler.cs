using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ItemGroups.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ItemGroups.Handlers
{
    public class GetItemGroupByIdQueryHandler : IRequestHandler<GetItemGroupByIdQuery, ItemGroupViewModel?>
    {
        private readonly IRepository<ItemGroupEntity> _itemGroupRepository;
        private readonly IRepository<ItemEntity> _itemRepository;

        public GetItemGroupByIdQueryHandler(
            IRepository<ItemGroupEntity> itemGroupRepository,
            IRepository<ItemEntity> itemRepository)
        {
            _itemGroupRepository = itemGroupRepository;
            _itemRepository = itemRepository;
        }

        public async Task<ItemGroupViewModel?> Handle(GetItemGroupByIdQuery request, CancellationToken cancellationToken)
        {
            return await _itemGroupRepository.Query()
                .Where(x => x.ItemGroupId == request.Id && !x.IsDeleted)
                .Select(x => new ItemGroupViewModel
                {
                    ItemGroupId = x.ItemGroupId,
                    GroupCode = x.GroupCode,
                    GroupName = x.GroupName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    ItemCount = _itemRepository.Query().Count(i => i.ItemGroupId == x.ItemGroupId && !i.IsDeleted)
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}