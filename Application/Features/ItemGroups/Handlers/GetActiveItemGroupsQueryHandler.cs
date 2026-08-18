using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ItemGroups.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ItemGroups.Handlers
{
    public class GetActiveItemGroupsQueryHandler : IRequestHandler<GetActiveItemGroupsQuery, List<ItemGroupViewModel>>
    {
        private readonly IRepository<ItemGroupEntity> _itemGroupRepository;

        public GetActiveItemGroupsQueryHandler(IRepository<ItemGroupEntity> itemGroupRepository)
        {
            _itemGroupRepository = itemGroupRepository;
        }

        public async Task<List<ItemGroupViewModel>> Handle(GetActiveItemGroupsQuery request, CancellationToken cancellationToken)
        {
            return await _itemGroupRepository.Query()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.GroupCode)
                .Select(x => new ItemGroupViewModel
                {
                    ItemGroupId = x.ItemGroupId,
                    GroupCode = x.GroupCode,
                    GroupName = x.GroupName,
                    Description = x.Description,
                    IsActive = x.IsActive
                }).ToListAsync(cancellationToken);
        }
    }
}