using AccuFlow.Entities.Entity;

namespace AccuFlow.Entities.Seeders
{
    public static class MenuSeed
    {
        public static List<MenuEntity> GetMenuSeedData()
        {
            return new List<MenuEntity>
            {
                // Dashboard
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
                
                // Database Seeding
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    MenuParentId = null,
                    Icon = @"<i class=""ki-duotone ki-abstract-26 fs-2""><span class=""path1""></span><span class=""path2""></span></i>",
                    Name = "Database Seeding",
                    Controller = "Seed",
                    Action = @"[""view""]",
                    Sequence = 2
                },
                
                // User Management (Parent)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    MenuParentId = null,
                    Icon = @"<i class=""ki-duotone ki-profile-user fs-2""><span class=""path1""></span><span class=""path2""></span><span class=""path3""></span><span class=""path4""></span></i>",
                    Name = "User Management",
                    Controller = "",
                    Action = @"[]",
                    Sequence = 3
                },
                
                // Users (Child of User Management)
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
                
                // Roles (Child of User Management)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    Icon = "",
                    Name = "Roles",
                    Controller = "Role",
                    Action = @"[""view"",""add"",""edit"",""delete""]",
                    Sequence = 2
                },
                
                // Master (Parent)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000006"),
                    MenuParentId = null,
                    Icon = @"<i class=""ki-duotone ki-category fs-2""><span class=""path1""></span><span class=""path2""></span><span class=""path3""></span><span class=""path4""></span></i>",
                    Name = "Master",
                    Controller = "",
                    Action = @"[]",
                    Sequence = 4
                },
                
                // Chart of Accounts (Child of Master)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000007"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000006"),
                    Icon = "",
                    Name = "Chart of Accounts",
                    Controller = "ChartOfAccount",
                    Action = @"[""view"",""add"",""edit"",""delete""]",
                    Sequence = 1
                },
                
                // Customers (Child of Master)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000008"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000006"),
                    Icon = "",
                    Name = "Customers",
                    Controller = "Customer",
                    Action = @"[""view"",""add"",""edit"",""delete""]",
                    Sequence = 2
                },
                
                // Accounting (Parent)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    MenuParentId = null,
                    Icon = @"<i class=""ki-duotone ki-calculator fs-2""><span class=""path1""></span><span class=""path2""></span><span class=""path3""></span></i>",
                    Name = "Accounting",
                    Controller = "",
                    Action = @"[]",
                    Sequence = 5
                },
                
                // Journal Entry (Child of Accounting)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Icon = "",
                    Name = "Journal Entry",
                    Controller = "JournalEntry",
                    Action = @"[""view"",""add"",""edit"",""delete"",""post"",""reverse""]",
                    Sequence = 1
                },
                
                // General Ledger (Child of Accounting)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000012"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Icon = "",
                    Name = "General Ledger",
                    Controller = "GeneralLedger",
                    Action = @"[""view""]",
                    Sequence = 2
                },
                
                // Trial Balance (Child of Accounting)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000013"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Icon = "",
                    Name = "Trial Balance",
                    Controller = "TrialBalance",
                    Action = @"[""view""]",
                    Sequence = 3
                },
                
                // Financial Statements (Child of Accounting)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000014"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Icon = "",
                    Name = "Financial Statements",
                    Controller = "FinancialStatement",
                    Action = @"[""view""]",
                    Sequence = 4
                }
            };
        }
    }
}
