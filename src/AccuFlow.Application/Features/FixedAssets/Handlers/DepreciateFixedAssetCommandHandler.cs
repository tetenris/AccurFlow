using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.FixedAssets.Commands;
using AccuFlow.Application.Features.JournalEntries.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.FixedAsset;
using AccuFlow.Models.JournalEntry;
using MediatR;

namespace AccuFlow.Application.Features.FixedAssets.Handlers
{
    public class DepreciateFixedAssetCommandHandler : IRequestHandler<DepreciateFixedAssetCommand, FixedAssetDepreciationViewModel>
    {
        private static readonly Guid DefaultAccumulatedDepreciationAccountId = Guid.Parse("10000000-0000-0000-0000-000000000007");
        private static readonly Guid DefaultDepreciationExpenseAccountId = Guid.Parse("50000000-0000-0000-0000-000000000006");

        private readonly IRepository<FixedAssetEntity> _fixedAssetRepository;
        private readonly IRepository<FixedAssetDepreciationEntity> _depreciationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISender _mediator;

        public DepreciateFixedAssetCommandHandler(
            IRepository<FixedAssetEntity> fixedAssetRepository,
            IRepository<FixedAssetDepreciationEntity> depreciationRepository,
            IUnitOfWork unitOfWork,
            ISender mediator)
        {
            _fixedAssetRepository = fixedAssetRepository;
            _depreciationRepository = depreciationRepository;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<FixedAssetDepreciationViewModel> Handle(DepreciateFixedAssetCommand request, CancellationToken cancellationToken)
        {
            var asset = await _fixedAssetRepository.FirstOrDefaultAsync(x => x.AssetId == request.AssetId && !x.IsDeleted, cancellationToken);
            if (asset == null) throw new Exception("Fixed asset not found");
            if (asset.Status != "Active") throw new Exception("Only active asset can be depreciated");

            var monthly = (asset.PurchaseCost - asset.SalvageValue) / asset.UsefulLifeMonths;
            var lastPeriod = asset.LastDepreciationDate ?? asset.PurchaseDate.AddMonths(-1);
            var monthsToRun = ((request.PeriodDate.Year - lastPeriod.Year) * 12) + (request.PeriodDate.Month - lastPeriod.Month);
            if (monthsToRun <= 0) throw new Exception("No depreciation due for this period");
            var capMonths = asset.UsefulLifeMonths;
            var runMonths = Math.Min(monthsToRun, capMonths);
            if (runMonths <= 0) throw new Exception("Asset is fully depreciated");

            var amount = decimal.Round(monthly * runMonths, 2);
            var usable = asset.PurchaseCost - asset.SalvageValue - asset.AccumulatedDepreciation;
            if (amount > usable) amount = usable;
            if (amount <= 0) throw new Exception("Asset is fully depreciated");

            var journalLines = new List<JournalLineRequest>
            {
                new JournalLineRequest
                {
                    AccountId = asset.DepreciationExpenseAccountId ?? DefaultDepreciationExpenseAccountId,
                    Description = $"Depreciation for {asset.AssetCode} - {asset.AssetName}",
                    DebitAmount = amount,
                    CreditAmount = 0
                },
                new JournalLineRequest
                {
                    AccountId = asset.AccumulatedDepreciationAccountId ?? DefaultAccumulatedDepreciationAccountId,
                    Description = $"Depreciation for {asset.AssetCode} - {asset.AssetName}",
                    DebitAmount = 0,
                    CreditAmount = amount
                }
            };
            var journalId = await _mediator.Send(new CreateJournalCommand(new CreateJournalEntryRequest
            {
                JournalDate = request.PeriodDate,
                Description = $"Depreciation for {asset.AssetCode} - {asset.AssetName}",
                JournalLines = journalLines
            }, request.UserId), cancellationToken);
            await _mediator.Send(new PostJournalCommand(new PostJournalRequest { JournalId = journalId, PostedDate = request.PeriodDate }, request.UserId), cancellationToken);

            asset.AccumulatedDepreciation += amount;
            asset.LastDepreciationDate = request.PeriodDate;
            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = request.UserId.ToString();
            var deprec = new FixedAssetDepreciationEntity
            {
                DepreciationId = Guid.NewGuid(),
                AssetId = asset.AssetId,
                PeriodDate = request.PeriodDate,
                Amount = amount,
                JournalId = journalId,
                CreatedBy = request.UserId.ToString()
            };
            _fixedAssetRepository.Update(asset);
            _depreciationRepository.Add(deprec);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new FixedAssetDepreciationViewModel
            {
                DepreciationId = deprec.DepreciationId,
                PeriodDate = deprec.PeriodDate,
                Amount = deprec.Amount,
                JournalId = deprec.JournalId
            };
        }
    }
}