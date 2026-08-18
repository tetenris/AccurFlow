using AccuFlow.Models.Inventory;
using MediatR;

namespace AccuFlow.Application.Features.Units.Commands
{
    public record UpdateUnitCommand(UpdateUnitRequest Request, Guid UserId) : IRequest;
}