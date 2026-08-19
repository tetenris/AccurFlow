using AccuFlow.Models.BaseModel;
using MediatR;

namespace AccuFlow.Application.Features.BillOfMaterials.Queries
{
    public record GetBomDatatableQuery(BaseDatatableRequest Request) : IRequest<BaseDatatableResponse>;
}