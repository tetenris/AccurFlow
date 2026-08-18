using MediatR;
using AccuFlow.Models.RoleMenu;

namespace AccuFlow.Application.Features.Roles.Commands
{
    public record SaveRoleMenuPermissionsCommand(SaveRoleMenuRequest Request) : IRequest;
}