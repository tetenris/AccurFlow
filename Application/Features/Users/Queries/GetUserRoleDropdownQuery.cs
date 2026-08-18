using MediatR;
using AccuFlow.Models.Role;

namespace AccuFlow.Application.Features.Users.Queries
{
    public record GetUserRoleDropdownQuery() : IRequest<List<RoleViewModel>>;
}