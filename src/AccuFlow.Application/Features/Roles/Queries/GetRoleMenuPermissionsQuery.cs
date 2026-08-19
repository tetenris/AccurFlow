using MediatR;
using AccuFlow.Models.RoleMenu;

namespace AccuFlow.Application.Features.Roles.Queries
{
    public record GetRoleMenuPermissionsQuery(Guid RoleId) : IRequest<RoleMenuViewModel>;
}