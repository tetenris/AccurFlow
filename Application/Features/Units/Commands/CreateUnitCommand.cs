using AccuFlow.Models.Inventory;
using MediatR;

namespace AccuFlow.Application.Features.Units.Commands
{
    public record CreateUnitCommand(CreateUnitRequest Request, Guid UserId) : IRequest;
}