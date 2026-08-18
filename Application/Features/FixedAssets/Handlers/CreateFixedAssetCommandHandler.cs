using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.FixedAssets.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.FixedAssets.Handlers
{
    public class CreateFixedAssetCommandHandler : IRequestHandler<CreateFixedAssetCommand>
    {
        private static readonly Guid DefaultAssetAccountId = Guid.Parse("10000000-0000-0000-0000-000000000006");
        private static readonly Guid DefaultAccumulatedDepreciationAccountId = Guid.Parse("10000000-0000-0000-0000-000000000007");
        private static readonly Guid DefaultDepreciationExpenseAccountId = Guid.Parse("50000000-0000-0000-0000-000000000006");

        private readonly IRepository<FixedAssetEntity> _fixedAssetRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateFixedAssetCommandHandler(
            IRepository<FixedAssetEntity> fixedAssetRepository,
            IUnitOfWork unitOfWork)
        {
            _fixedAssetRepository = fixedAssetRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateFixedAssetCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            if (r.PurchaseCost <= 0) throw new Exception("Purchase cost must be greater than zero");
            if (r.SalvageValue >= r.PurchaseCost) throw new Exception("Salvage value must be less than purchase cost");
            if (r.UsefulLifeMonths <= 0) throw new Exception("Useful life must be greater than zero");

            _fixedAssetRepository.Add(new FixedAssetEntity
            {
                AssetId = Guid.NewGuid(),
                AssetCode = await GenerateNumberAsync(cancellationToken),
                AssetName = r.AssetName,
                Category = r.Category,
                PurchaseDate = r.PurchaseDate,
                PurchaseCost = r.PurchaseCost,
                SalvageValue = r.SalvageValue,
                UsefulLifeMonths = r.UsefulLifeMonths,
                AssetAccountId = r.AssetAccountId ?? DefaultAssetAccountId,
                AccumulatedDepreciationAccountId = r.AccumulatedDepreciationAccountId ?? DefaultAccumulatedDepreciationAccountId,
                DepreciationExpenseAccountId = r.DepreciationExpenseAccountId ?? DefaultDepreciationExpenseAccountId,
                Status = "Active",
                Notes = r.Notes,
                CreatedBy = userId.ToString()
            });
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<string> GenerateNumberAsync(CancellationToken cancellationToken)
        {
            var last = await _fixedAssetRepository.Query()
                .Where(x => x.AssetCode.StartsWith("FA-"))
                .OrderByDescending(x => x.AssetCode).Select(x => x.AssetCode)
                .FirstOrDefaultAsync(cancellationToken);
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[3..]) + 1;
            return $"FA-{next:D5}";
        }
    }
}