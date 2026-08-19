using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Menus.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Entities.Enums;
using AccuFlow.Models.Menu;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AccuFlow.Application.Features.Menus.Handlers
{
    public class GetMenuHierarchyQueryHandler : IRequestHandler<GetMenuHierarchyQuery, List<MenuViewModel>>
    {
        private readonly IRepository<MenuEntity> _menuRepository;
        private readonly IRepository<RoleEntity> _roleRepository;
        private readonly IRepository<RoleMenuEntity> _roleMenuRepository;

        public GetMenuHierarchyQueryHandler(
            IRepository<MenuEntity> menuRepository,
            IRepository<RoleEntity> roleRepository,
            IRepository<RoleMenuEntity> roleMenuRepository)
        {
            _menuRepository = menuRepository;
            _roleRepository = roleRepository;
            _roleMenuRepository = roleMenuRepository;
        }

        public async Task<List<MenuViewModel>> Handle(GetMenuHierarchyQuery request, CancellationToken cancellationToken)
        {
            if (!request.RoleId.HasValue)
                return await GetMenuHierarchyAsync(cancellationToken);

            var roleId = request.RoleId.Value;
            var role = await _roleRepository.FirstOrDefaultAsync(r => r.RoleId == roleId && !r.IsDeleted, cancellationToken);

            var isAdministrator = role?.RoleType == RoleEnum.SuperAdministrator
                || role?.RoleType == RoleEnum.Administrator;

            if (isAdministrator)
                return await GetMenuHierarchyAsync(cancellationToken);

            var roleMenuPermissions = await _roleMenuRepository
                .FindAsync(rm => rm.RoleId == roleId && !rm.IsDeleted, cancellationToken);

            if (!roleMenuPermissions.Any())
                return new List<MenuViewModel>();

            var allMenus = await _menuRepository.Query()
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.Sequence)
                .ToListAsync(cancellationToken);

            var allowedMenuIds = roleMenuPermissions
                .Where(rm => rm.CanView)
                .Select(rm => rm.MenuId)
                .ToList();

            var parentMenuIds = allMenus
                .Where(m => allowedMenuIds.Contains(m.MenuId) && m.MenuParentId.HasValue)
                .Select(m => m.MenuParentId!.Value)
                .Distinct()
                .ToList();

            allowedMenuIds.AddRange(parentMenuIds.Where(id => !allowedMenuIds.Contains(id)));

            var allowedMenus = allMenus.Where(m => allowedMenuIds.Contains(m.MenuId)).ToList();

            var menuViewModels = allowedMenus.Select(m => new MenuViewModel
            {
                MenuId = m.MenuId,
                MenuParentId = m.MenuParentId,
                Icon = m.Icon,
                Name = m.Name,
                Controller = m.Controller,
                Actions = ParseActions(m.Action),
                Sequence = m.Sequence
            }).ToList();

            var rootMenus = menuViewModels.Where(m => m.MenuParentId == null).ToList();

            foreach (var rootMenu in rootMenus)
            {
                rootMenu.ChildMenus = menuViewModels
                    .Where(m => m.MenuParentId == rootMenu.MenuId)
                    .OrderBy(m => m.Sequence)
                    .ToList();
            }

            return rootMenus;
        }

        private async Task<List<MenuViewModel>> GetMenuHierarchyAsync(CancellationToken cancellationToken)
        {
            var allMenus = await _menuRepository.Query()
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.Sequence)
                .ToListAsync(cancellationToken);

            var menuViewModels = allMenus.Select(m => new MenuViewModel
            {
                MenuId = m.MenuId,
                MenuParentId = m.MenuParentId,
                Icon = m.Icon,
                Name = m.Name,
                Controller = m.Controller,
                Actions = ParseActions(m.Action),
                Sequence = m.Sequence
            }).ToList();

            var rootMenus = menuViewModels.Where(m => m.MenuParentId == null).ToList();

            foreach (var rootMenu in rootMenus)
            {
                rootMenu.ChildMenus = menuViewModels
                    .Where(m => m.MenuParentId == rootMenu.MenuId)
                    .OrderBy(m => m.Sequence)
                    .ToList();
            }

            return rootMenus;
        }

        private List<string> ParseActions(string actionJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(actionJson))
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