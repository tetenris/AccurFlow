using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.FixedAssets.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.FixedAsset;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.FixedAssets.Handlers
{
    public class GetFixedAssetByIdQueryHandler : IRequestHandler<GetFixedAssetByIdQuery, FixedAssetDetailViewModel?>
    {
        private readonly IRepository<FixedAssetEntity> _fixedAssetRepository;

        public GetFixedAssetByIdQueryHandler(IRepository<FixedAssetEntity> fixedAssetRepository)
        {
            _fixedAssetRepository = fixedAssetRepository;
        }

        public async Task<FixedAssetDetailViewModel?> Handle(GetFixedAssetByIdQuery request, CancellationToken cancellationToken)
        {
            return await _fixedAssetRepository.Query()
                .Include(x => x.Depreciations.Where(d => !d.IsDeleted))
                .Where(x => x.AssetId == request.AssetId && !x.IsDeleted)
                .Select(x => new FixedAssetDetailViewModel
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
                    Status = x.Status,
                    AssetAccountId = x.AssetAccountId,
                    AccumulatedDepreciationAccountId = x.AccumulatedDepreciationAccountId,
                    DepreciationExpenseAccountId = x.DepreciationExpenseAccountId,
                    LastDepreciationDate = x.LastDepreciationDate,
                    Notes = x.Notes,
                    Depreciations = x.Depreciations.Select(d => new FixedAssetDepreciationViewModel
                    {
                        DepreciationId = d.DepreciationId,
                        PeriodDate = d.PeriodDate,
                        Amount = d.Amount,
                        JournalId = d.JournalId
                    }).ToList()
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}