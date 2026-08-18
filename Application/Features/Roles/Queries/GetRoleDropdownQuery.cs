using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AccuFlow.Application.Features.Roles.Queries
{
    public record GetRoleDropdownQuery() : IRequest<List<SelectListItem>>;
}