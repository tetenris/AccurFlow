using MediatR;

namespace AccuFlow.Application.Features.ItemGroups.Commands
{
    public record DeleteItemGroupCommand(Guid Id, Guid UserId) : IRequest;
}