using AccuFlow.Models.FixedAsset;
using MediatR;

namespace AccuFlow.Application.Features.FixedAssets.Commands
{
    public record DepreciateFixedAssetCommand(Guid AssetId, DateTime PeriodDate, Guid UserId) : IRequest<FixedAssetDepreciationViewModel>;
}