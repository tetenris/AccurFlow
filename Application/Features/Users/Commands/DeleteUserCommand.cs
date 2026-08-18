using MediatR;

namespace AccuFlow.Application.Features.Users.Commands
{
    public record DeleteUserCommand(Guid Id) : IRequest;
}