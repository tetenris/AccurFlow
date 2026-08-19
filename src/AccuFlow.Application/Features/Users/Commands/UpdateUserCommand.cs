using MediatR;
using AccuFlow.Models.User;

namespace AccuFlow.Application.Features.Users.Commands
{
    public record UpdateUserCommand(UpdateUserViewModel Model) : IRequest;
}