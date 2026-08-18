using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ItemGroups.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ItemGroups.Handlers
{
    public class GetItemGroupDatatableQueryHandler : IRequestHandler<GetItemGroupDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<ItemGroupEntity> _itemGroupRepository;
        private readonly IRepository<ItemEntity> _itemRepository;

        public GetItemGroupDatatableQueryHandler(
            IRepository<ItemGroupEntity> itemGroupRepository,
            IRepository<ItemEntity> itemRepository)
        {
            _itemGroupRepository = itemGroupRepository;
            _itemRepository = itemRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetItemGroupDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _itemGroupRepository.Query().Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.GroupCode.ToLower().Contains(search) || x.GroupName.ToLower().Contains(search));
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderBy(x => x.GroupCode).Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new ItemGroupViewModel
                {
                    ItemGroupId = x.ItemGroupId,
                    GroupCode = x.GroupCode,
                    GroupName = x.GroupName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    ItemCount = _itemRepository.Query().Count(i => i.ItemGroupId == x.ItemGroupId && !i.IsDeleted)
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}