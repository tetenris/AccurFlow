using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Roles.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.RoleMenu;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AccuFlow.Application.Features.Roles.Handlers
{
    public class GetRoleMenuPermissionsQueryHandler : IRequestHandler<GetRoleMenuPermissionsQuery, RoleMenuViewModel>
    {
        private readonly IRepository<MenuEntity> _menuRepository;
        private readonly IRepository<RoleMenuEntity> _roleMenuRepository;

        public GetRoleMenuPermissionsQueryHandler(
            IRepository<MenuEntity> menuRepository,
            IRepository<RoleMenuEntity> roleMenuRepository)
        {
            _menuRepository = menuRepository;
            _roleMenuRepository = roleMenuRepository;
        }

        public async Task<RoleMenuViewModel> Handle(GetRoleMenuPermissionsQuery request, CancellationToken cancellationToken)
        {
            var menus = await _menuRepository.Query()
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.Sequence)
                .ToListAsync(cancellationToken);

            List<RoleMenuEntity> roleMenus = new List<RoleMenuEntity>();

            if (request.RoleId != Guid.Empty)
            {
                roleMenus = await _roleMenuRepository.Query()
                    .Where(rm => rm.RoleId == request.RoleId && !rm.IsDeleted)
                    .ToListAsync(cancellationToken);
            }

            var menuPermissions = menus.Select(menu =>
            {
                var roleMenu = roleMenus.FirstOrDefault(rm => rm.MenuId == menu.MenuId);
                var availableActions = ParseActions(menu.Action);

                return new MenuPermissionViewModel
                {
                    MenuId = menu.MenuId,
                    MenuName = menu.Name,
                    ParentMenuId = menu.MenuParentId,
                    CanView = roleMenu?.CanView ?? false,
                    CanAdd = roleMenu?.CanAdd ?? false,
                    CanEdit = roleMenu?.CanEdit ?? false,
                    CanDelete = roleMenu?.CanDelete ?? false,
                    CanPost = roleMenu?.CanPost ?? false,
                    CanReverse = roleMenu?.CanReverse ?? false,
                    AvailableActions = availableActions
                };
            }).ToList();

            return new RoleMenuViewModel
            {
                RoleId = request.RoleId,
                Menus = menuPermissions
            };
        }

        private static List<string> ParseActions(string actionJson)
        {
            try
            {
                if (string.IsNullOrEmpty(actionJson))
                    return new List<string>();

                return JsonSerializer.Deserialize<List<string>>(actionJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}