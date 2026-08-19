using AccuFlow.Models.Inventory;
using MediatR;

namespace AccuFlow.Application.Features.ItemGroups.Queries
{
    public record GetItemGroupByIdQuery(Guid Id) : IRequest<ItemGroupViewModel?>;
}