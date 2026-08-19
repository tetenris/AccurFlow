using MediatR;

namespace AccuFlow.Application.Features.Account.Commands
{
    public record ChangePasswordCommand(
        Guid UserId,
        string CurrentPassword,
        string NewPassword) : IRequest;
}