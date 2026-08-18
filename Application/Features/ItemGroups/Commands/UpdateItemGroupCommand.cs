using AccuFlow.Models.Inventory;
using MediatR;

namespace AccuFlow.Application.Features.ItemGroups.Commands
{
    public record UpdateItemGroupCommand(UpdateItemGroupRequest Request, Guid UserId) : IRequest;
}