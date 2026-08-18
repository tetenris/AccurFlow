using AccuFlow.Domain.Entities;

namespace AccuFlow.Entities.Seeders
{
    public static class RoleMenuSeed
    {
        /// <summary>
        /// Seed default permissions for Administrator role (all menus with full access)
        /// </summary>
        public static List<RoleMenuEntity> GetDefaultAdminPermissions()
        {
            var adminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            var menus = MenuSeed.GetMenuSeedData();
            var permissions = new List<RoleMenuEntity>();

            foreach (var menu in menus)
            {
                permissions.Add(new RoleMenuEntity
                {
                    RoleMenuId = Guid.NewGuid(),
                    RoleId = adminRoleId,
                    MenuId = menu.MenuId,
                    CanView = true,
                    CanAdd = true,
                    CanEdit = true,
                    CanDelete = true,
                    CanPost = true,
                    CanReverse = true,
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                });
            }

            return permissions;
        }
    }
}

