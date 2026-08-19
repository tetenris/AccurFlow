using MediatR;
using AccuFlow.Models.Role;

namespace AccuFlow.Application.Features.Roles.Commands
{
    public record EditRoleCommand(EditRoleRequest Request, Guid UserId) : IRequest;
}