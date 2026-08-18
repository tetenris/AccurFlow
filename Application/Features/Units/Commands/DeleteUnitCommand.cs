using MediatR;

namespace AccuFlow.Application.Features.Units.Commands
{
    public record DeleteUnitCommand(Guid Id, Guid UserId) : IRequest;
}