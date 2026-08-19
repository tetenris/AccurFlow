using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Inventory;
using MediatR;

namespace AccuFlow.Application.Features.Items.Queries
{
    public record GetItemsDatatableQuery(DataTableItemRequest Request) : IRequest<BaseDatatableResponse>;
}