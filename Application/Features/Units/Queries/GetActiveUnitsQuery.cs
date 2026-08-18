using AccuFlow.Models.Inventory;
using MediatR;

namespace AccuFlow.Application.Features.Units.Queries
{
    public record GetActiveUnitsQuery() : IRequest<List<UnitViewModel>>;
}