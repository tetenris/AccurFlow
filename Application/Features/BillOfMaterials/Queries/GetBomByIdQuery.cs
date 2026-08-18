using AccuFlow.Models.Production;
using MediatR;

namespace AccuFlow.Application.Features.BillOfMaterials.Queries
{
    public record GetBomByIdQuery(Guid BomId) : IRequest<BomDetailViewModel?>;
}