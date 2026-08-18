using AccuFlow.Models.Inventory;
using MediatR;

namespace AccuFlow.Application.Features.ItemGroups.Commands
{
    public record CreateItemGroupCommand(CreateItemGroupRequest Request, Guid UserId) : IRequest;
}