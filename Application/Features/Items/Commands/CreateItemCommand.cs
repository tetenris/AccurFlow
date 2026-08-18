using AccuFlow.Models.Inventory;
using MediatR;

namespace AccuFlow.Application.Features.Items.Commands
{
    public record CreateItemCommand(CreateItemRequest Request, Guid UserId) : IRequest;
}