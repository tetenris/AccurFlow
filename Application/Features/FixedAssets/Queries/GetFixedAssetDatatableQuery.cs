using AccuFlow.Models.BaseModel;
using AccuFlow.Models.FixedAsset;
using MediatR;

namespace AccuFlow.Application.Features.FixedAssets.Queries
{
    public record GetFixedAssetDatatableQuery(DataTableFixedAssetRequest Request) : IRequest<BaseDatatableResponse>;
}