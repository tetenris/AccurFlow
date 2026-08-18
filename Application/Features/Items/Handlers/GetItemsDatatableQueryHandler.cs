using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Items.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Items.Handlers
{
    public class GetItemsDatatableQueryHandler : IRequestHandler<GetItemsDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<ItemEntity> _itemRepository;

        public GetItemsDatatableQueryHandler(IRepository<ItemEntity> itemRepository)
        {
            _itemRepository = itemRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetItemsDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _itemRepository.Query().Include(x => x.ItemGroup).Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(r.ItemType)) query = query.Where(x => x.ItemType == r.ItemType);
            if (r.IsActive.HasValue) query = query.Where(x => x.IsActive == r.IsActive.Value);
            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.ItemCode.ToLower().Contains(search) || x.ItemName.ToLower().Contains(search));
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderBy(x => x.ItemCode).Skip((r.Page - 1) * r.Size).Take(r.Size)
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
                    ReorderPoint = x.ReorderPoint,
                    IsActive = x.IsActive
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}