using AccuFlow.Models.StockOpname;
using MediatR;

namespace AccuFlow.Application.Features.StockOpnames.Queries
{
    public record GetStockOpnameQuantitiesQuery(Guid WarehouseId) : IRequest<List<StockOpnameLineViewModel>>;
}