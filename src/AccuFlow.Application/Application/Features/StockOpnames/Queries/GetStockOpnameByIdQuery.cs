using AccuFlow.Models.StockOpname;
using MediatR;

namespace AccuFlow.Application.Features.StockOpnames.Queries
{
    public record GetStockOpnameByIdQuery(Guid Id) : IRequest<StockOpnameDetailViewModel?>;
}