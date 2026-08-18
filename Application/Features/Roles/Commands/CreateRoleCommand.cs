using MediatR;
using AccuFlow.Models.Role;

namespace AccuFlow.Application.Features.Roles.Commands
{
    public record CreateRoleCommand(CreateRoleRequest Request, Guid UserId) : IRequest<Guid>;
}