using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using AccuFlow.Models.RoleMenu;
using AccuFlow.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AccuFlow.Services;

public class RoleMenuService : BaseService, IRoleMenuService
{
    public RoleMenuService(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<RoleMenuViewModel> GetRoleMenuPermissionsAsync(Guid roleId)
    {
        var menus = await _dbContext.Set<MenuEntity>()
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.Sequence)
            .ToListAsync();

        List<RoleMenuEntity> roleMenus = new List<RoleMenuEntity>();
        
        // Only load permissions if roleId is not empty (for edit mode)
        if (roleId != Guid.Empty)
        {
            roleMenus = await _dbContext.Set<RoleMenuEntity>()
                .Where(rm => rm.RoleId == roleId && !rm.IsDeleted)
                .ToListAsync();
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
            RoleId = roleId,
            Menus = menuPermissions
        };
    }

    public async Task SaveRoleMenuPermissionsAsync(SaveRoleMenuRequest request)
    {
        // Delete existing permissions
        var existingPermissions = await _dbContext.Set<RoleMenuEntity>()
            .Where(rm => rm.RoleId == request.RoleId)
            .ToListAsync();

        _dbContext.Set<RoleMenuEntity>().RemoveRange(existingPermissions);

        // Add new permissions - Save ALL menus (even with all false)
        // This way we can distinguish between "no permissions set" vs "explicitly denied"
        foreach (var permission in request.Permissions)
        {
            var roleMenu = new RoleMenuEntity
            {
                RoleId = request.RoleId,
                MenuId = permission.MenuId,
                CanView = permission.CanView,
                CanAdd = permission.CanAdd,
                CanEdit = permission.CanEdit,
                CanDelete = permission.CanDelete,
                CanPost = permission.CanPost,
                CanReverse = permission.CanReverse
            };

            await _dbContext.Set<RoleMenuEntity>().AddAsync(roleMenu);
        }

        await _dbContext.SaveChangesAsync();
    }

    private List<string> ParseActions(string actionJson)
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
