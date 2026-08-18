using AccuFlow.Models.BaseModel;
using AccuFlow.Models.StockOpname;
using MediatR;

namespace AccuFlow.Application.Features.StockOpnames.Queries
{
    public record GetStockOpnameDatatableQuery(DataTableStockOpnameRequest Request) : IRequest<BaseDatatableResponse>;
}