using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.FixedAssets.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.FixedAsset;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.FixedAssets.Handlers
{
    public class GetFixedAssetDatatableQueryHandler : IRequestHandler<GetFixedAssetDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<FixedAssetEntity> _fixedAssetRepository;

        public GetFixedAssetDatatableQueryHandler(IRepository<FixedAssetEntity> fixedAssetRepository)
        {
            _fixedAssetRepository = fixedAssetRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetFixedAssetDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _fixedAssetRepository.Query().Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(r.Category)) query = query.Where(x => x.Category == r.Category);
            if (!string.IsNullOrWhiteSpace(r.Status)) query = query.Where(x => x.Status == r.Status);
            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.AssetCode.ToLower().Contains(search) || x.AssetName.ToLower().Contains(search));
            }
            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderByDescending(x => x.PurchaseDate).Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new FixedAssetViewModel
                {
                    AssetId = x.AssetId,
                    AssetCode = x.AssetCode,
                    AssetName = x.AssetName,
                    Category = x.Category,
                    PurchaseDate = x.PurchaseDate,
                    PurchaseCost = x.PurchaseCost,
                    SalvageValue = x.SalvageValue,
                    UsefulLifeMonths = x.UsefulLifeMonths,
                    AccumulatedDepreciation = x.AccumulatedDepreciation,
                    Status = x.Status
                }).ToListAsync(cancellationToken);
            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}