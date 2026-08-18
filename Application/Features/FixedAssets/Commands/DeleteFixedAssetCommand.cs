using MediatR;

namespace AccuFlow.Application.Features.FixedAssets.Commands
{
    public record DeleteFixedAssetCommand(Guid AssetId, Guid UserId) : IRequest;
}