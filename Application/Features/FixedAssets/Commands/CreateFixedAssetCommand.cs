using AccuFlow.Models.FixedAsset;
using MediatR;

namespace AccuFlow.Application.Features.FixedAssets.Commands
{
    public record CreateFixedAssetCommand(CreateFixedAssetRequest Request, Guid UserId) : IRequest;
}