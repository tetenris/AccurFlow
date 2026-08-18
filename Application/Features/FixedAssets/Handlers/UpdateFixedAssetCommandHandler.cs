using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.FixedAssets.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.FixedAssets.Handlers
{
    public class UpdateFixedAssetCommandHandler : IRequestHandler<UpdateFixedAssetCommand>
    {
        private readonly IRepository<FixedAssetEntity> _fixedAssetRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateFixedAssetCommandHandler(
            IRepository<FixedAssetEntity> fixedAssetRepository,
            IUnitOfWork unitOfWork)
        {
            _fixedAssetRepository = fixedAssetRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateFixedAssetCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var asset = await _fixedAssetRepository.FirstOrDefaultAsync(x => x.AssetId == r.AssetId && !x.IsDeleted, cancellationToken);
            if (asset == null) throw new Exception("Fixed asset not found");
            if (r.PurchaseCost <= 0) throw new Exception("Purchase cost must be greater than zero");
            if (r.SalvageValue >= r.PurchaseCost) throw new Exception("Salvage value must be less than purchase cost");
            if (r.UsefulLifeMonths <= 0) throw new Exception("Useful life must be greater than zero");

            asset.AssetName = r.AssetName;
            asset.Category = r.Category;
            asset.PurchaseDate = r.PurchaseDate;
            asset.PurchaseCost = r.PurchaseCost;
            asset.SalvageValue = r.SalvageValue;
            asset.UsefulLifeMonths = r.UsefulLifeMonths;
            asset.AssetAccountId = r.AssetAccountId ?? asset.AssetAccountId;
            asset.AccumulatedDepreciationAccountId = r.AccumulatedDepreciationAccountId ?? asset.AccumulatedDepreciationAccountId;
            asset.DepreciationExpenseAccountId = r.DepreciationExpenseAccountId ?? asset.DepreciationExpenseAccountId;
            asset.Notes = r.Notes;
            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = userId.ToString();
            _fixedAssetRepository.Update(asset);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}