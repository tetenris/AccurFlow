using AccuFlow.Entities.Entity;

namespace AccuFlow.Entities.Seeders
{
    public static class MenuSeed
    {
        public static List<MenuEntity> GetMenuSeedData()
        {
            return new List<MenuEntity>
            {
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    MenuParentId = null,
                    Icon = @"<i class=""ki-duotone ki-element-11 fs-2""><span class=""path1""></span><span class=""path2""></span><span class=""path3""></span><span class=""path4""></span></i>",
                    Name = "Dashboard",
                    Controller = "Home",
                    Action = @"[""view""]",
                    Sequence = 1
                },
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    MenuParentId = null,
                    Icon = @"<i class=""ki-duotone ki-profile-user fs-2""><span class=""path1""></span><span class=""path2""></span><span class=""path3""></span><span class=""path4""></span></i>",
                    Name = "User Management",
                    Controller = "",
                    Action = @"[]",
                    Sequence = 2
                },
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    Icon = "",
                    Name = "Users",
                    Controller = "User",
                    Action = @"[""view"",""add"",""edit"",""delete""]",
                    Sequence = 1
                },
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    Icon = "",
                    Name = "Roles",
                    Controller = "Role",
                    Action = @"[""view"",""add"",""edit"",""delete""]",
                    Sequence = 2
                }
            };
        }
    }
}
