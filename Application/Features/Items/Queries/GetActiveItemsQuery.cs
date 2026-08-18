using AccuFlow.Models.Inventory;
using MediatR;

namespace AccuFlow.Application.Features.Items.Queries
{
    public record GetActiveItemsQuery : IRequest<List<ItemViewModel>>;
}