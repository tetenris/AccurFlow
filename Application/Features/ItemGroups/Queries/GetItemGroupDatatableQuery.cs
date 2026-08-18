using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Inventory;
using MediatR;

namespace AccuFlow.Application.Features.ItemGroups.Queries
{
    public record GetItemGroupDatatableQuery(DataTableItemGroupRequest Request) : IRequest<BaseDatatableResponse>;
}