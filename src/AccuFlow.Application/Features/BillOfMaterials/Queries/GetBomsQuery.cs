using AccuFlow.Models.Production;
using MediatR;

namespace AccuFlow.Application.Features.BillOfMaterials.Queries
{
    public record GetBomsQuery : IRequest<List<BomViewModel>>;
}