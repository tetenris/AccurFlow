using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Inventory;
using MediatR;

namespace AccuFlow.Application.Features.Items.Queries
{
    public record GetStockMinimumQuery(DataTableStockMinimumRequest Request) : IRequest<BaseDatatableResponse>;
}