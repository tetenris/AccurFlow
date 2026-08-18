using MediatR;

namespace AccuFlow.Application.Features.Account.Commands
{
    public record ForgotPasswordCommand(string Email) : IRequest<bool>;
}