using MediatR;
using AccuFlow.Application.Features.Account.Dtos;

namespace AccuFlow.Application.Features.Account.Commands
{
    public record LoginCommand(string Username, string Password) : IRequest<LoginResultDto>;
}