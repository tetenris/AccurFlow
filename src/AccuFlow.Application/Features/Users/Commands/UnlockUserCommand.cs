using MediatR;

namespace AccuFlow.Application.Features.Users.Commands
{
    public record UnlockUserCommand(Guid Id) : IRequest;
}