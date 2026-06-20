using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using AccuFlow.Models.Menu;
using AccuFlow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AccuFlow.Services
{
    public class MenuService : IMenuService
    {
        private readonly AppDbContext _dbContext;

        public MenuService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<MenuViewModel>> GetMenuHierarchyAsync()
        {
            var allMenus = await _dbContext.Menus
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.Sequence)
                .ToListAsync();

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

            // Build hierarchy
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

        public async Task<List<MenuViewModel>> GetMenuHierarchyByRoleAsync(Guid roleId)
        {
            // Get all menus
            var allMenus = await _dbContext.Menus
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.Sequence)
                .ToListAsync();

            // Check if this is Administrator role by RoleType
            var role = await _dbContext.Set<RoleEntity>()
                .Where(r => r.RoleId == roleId && !r.IsDeleted)
                .FirstOrDefaultAsync();
            
            var isAdministrator = role?.RoleType == Entities.Enums.RoleEnum.Administrator;

            // Administrator ALWAYS gets full access (bypass RoleMenu check)
            if (isAdministrator)
            {
                return await GetMenuHierarchyAsync();
            }

            // Get role permissions for non-Administrator roles
            var roleMenuPermissions = await _dbContext.Set<RoleMenuEntity>()
                .Where(rm => rm.RoleId == roleId && !rm.IsDeleted)
                .ToListAsync();

            // If no permissions found for non-Administrator
            if (!roleMenuPermissions.Any())
            {
                // Non-Administrator: show nothing (no access)
                return new List<MenuViewModel>();
            }

            // Filter menus based on permissions - show menus with CanView and include their parents
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

            // Build hierarchy - only include parent if it has permission
            var rootMenus = menuViewModels.Where(m => m.MenuParentId == null).ToList();
            
            foreach (var rootMenu in rootMenus)
            {
                var childMenus = menuViewModels
                    .Where(m => m.MenuParentId == rootMenu.MenuId)
                    .OrderBy(m => m.Sequence)
                    .ToList();
                
                rootMenu.ChildMenus = childMenus;
            }

            // Return only root menus that have permission
            // If parent doesn't have permission, its children won't show either
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
