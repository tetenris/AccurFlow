using MediatR;
using AccuFlow.Models.Role;

namespace AccuFlow.Application.Features.Roles.Queries
{
    public record GetRoleByIdQuery(Guid RoleId) : IRequest<RoleViewModel?>;
}