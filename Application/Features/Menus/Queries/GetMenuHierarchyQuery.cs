using MediatR;
using AccuFlow.Models.Menu;

namespace AccuFlow.Application.Features.Menus.Queries
{
    public record GetMenuHierarchyQuery(Guid? RoleId = null) : IRequest<List<MenuViewModel>>;
}