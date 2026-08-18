using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Inventory;
using MediatR;

namespace AccuFlow.Application.Features.Units.Queries
{
    public record GetUnitDatatableQuery(DataTableUnitRequest Request) : IRequest<BaseDatatableResponse>;
}