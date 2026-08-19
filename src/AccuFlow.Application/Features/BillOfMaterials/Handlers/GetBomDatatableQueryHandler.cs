using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.BillOfMaterials.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Production;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.BillOfMaterials.Handlers
{
    public class GetBomDatatableQueryHandler : IRequestHandler<GetBomDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<BillOfMaterialEntity> _bomRepository;

        public GetBomDatatableQueryHandler(IRepository<BillOfMaterialEntity> bomRepository)
        {
            _bomRepository = bomRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetBomDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _bomRepository.Query()
                .Include(x => x.FinishedItem)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.BomNumber.ToLower().Contains(search)
                    || x.FinishedItem.ItemCode.ToLower().Contains(search)
                    || x.FinishedItem.ItemName.ToLower().Contains(search));
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new BomViewModel
                {
                    BomId = x.BomId,
                    BomNumber = x.BomNumber,
                    FinishedItemId = x.FinishedItemId,
                    FinishedItemCode = x.FinishedItem.ItemCode,
                    FinishedItemName = x.FinishedItem.ItemName,
                    LineCount = x.Lines.Count,
                    IsActive = x.IsActive,
                    Notes = x.Notes
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}