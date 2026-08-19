using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.FixedAssets.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.FixedAssets.Handlers
{
    public class DeleteFixedAssetCommandHandler : IRequestHandler<DeleteFixedAssetCommand>
    {
        private readonly IRepository<FixedAssetEntity> _fixedAssetRepository;
        private readonly IRepository<FixedAssetDepreciationEntity> _depreciationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteFixedAssetCommandHandler(
            IRepository<FixedAssetEntity> fixedAssetRepository,
            IRepository<FixedAssetDepreciationEntity> depreciationRepository,
            IUnitOfWork unitOfWork)
        {
            _fixedAssetRepository = fixedAssetRepository;
            _depreciationRepository = depreciationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteFixedAssetCommand request, CancellationToken cancellationToken)
        {
            var asset = await _fixedAssetRepository.FirstOrDefaultAsync(x => x.AssetId == request.AssetId && !x.IsDeleted, cancellationToken);
            if (asset == null) throw new Exception("Fixed asset not found");
            if (await _depreciationRepository.AnyAsync(x => x.AssetId == request.AssetId && !x.IsDeleted, cancellationToken)) throw new Exception("Asset with depreciation history cannot be deleted");
            asset.IsDeleted = true;
            asset.DeletedAt = DateTime.UtcNow;
            asset.DeletedBy = request.UserId.ToString();
            _fixedAssetRepository.Update(asset);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}