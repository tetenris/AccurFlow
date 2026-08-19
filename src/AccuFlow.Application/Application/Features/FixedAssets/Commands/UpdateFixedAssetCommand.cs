using AccuFlow.Models.FixedAsset;
using MediatR;

namespace AccuFlow.Application.Features.FixedAssets.Commands
{
    public record UpdateFixedAssetCommand(UpdateFixedAssetRequest Request, Guid UserId) : IRequest;
}