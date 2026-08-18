using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Inventory;
using MediatR;

namespace AccuFlow.Application.Features.Items.Queries
{
    public record GetStockCardQuery(DataTableStockMovementRequest Request) : IRequest<BaseDatatableResponse>;
}