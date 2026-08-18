using AccuFlow.Domain.Entities;

namespace AccuFlow.Entities.Seeders
{
    public static class RoleSeed
    {
        /// <summary>
        /// Returns predefined role seed data for the application
        /// </summary>
        public static List<RoleEntity> GetRoleSeedData()
        {
            return new List<RoleEntity>
            {
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    RoleType = Enums.RoleEnum.SuperAdministrator,
                    RoleName = "Super Administrator",
                    Description = "Technical full system access",
                    Permissions = "[\"all\"]",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    RoleType = Enums.RoleEnum.Administrator,
                    RoleName = "Administrator",
                    Description = "Full operational access",
                    Permissions = "[\"all\"]",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    RoleType = Enums.RoleEnum.Manager,
                    RoleName = "Manager",
                    Description = "Management, report review, and approval access",
                    Permissions = "[\"view\",\"approve\"]",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                    RoleType = Enums.RoleEnum.Accountant,
                    RoleName = "Accountant",
                    Description = "Accounting operations, journal, posting, and financial reports",
                    Permissions = "[\"journal_entry\",\"invoice\",\"payment\",\"report\"]",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                    RoleType = Enums.RoleEnum.FinanceStaff,
                    RoleName = "Finance Staff",
                    Description = "Finance operational access for invoice and payment preparation",
                    Permissions = "[\"invoice\",\"payment\"]",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000006"),
                    RoleType = Enums.RoleEnum.AROfficer,
                    RoleName = "AR Officer",
                    Description = "Accounts receivable access for customer invoices, receipts, and AR aging",
                    Permissions = "[\"sales_invoice\",\"receipt\",\"ar_aging\"]",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000007"),
                    RoleType = Enums.RoleEnum.APOfficer,
                    RoleName = "AP Officer",
                    Description = "Accounts payable access for supplier invoices, payments, and AP aging",
                    Permissions = "[\"purchase_invoice\",\"supplier_payment\",\"ap_aging\"]",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000008"),
                    RoleType = Enums.RoleEnum.Purchasing,
                    RoleName = "Purchasing",
                    Description = "Purchasing access for suppliers and purchase orders",
                    Permissions = "[\"supplier\",\"purchase_order\"]",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000009"),
                    RoleType = Enums.RoleEnum.Sales,
                    RoleName = "Sales",
                    Description = "Sales access for customers and sales invoices",
                    Permissions = "[\"customer\",\"sales_invoice\"]",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    RoleType = Enums.RoleEnum.Warehouse,
                    RoleName = "Warehouse",
                    Description = "Inventory and warehouse operation access",
                    Permissions = "[\"inventory\",\"stock\"]",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                    RoleType = Enums.RoleEnum.Viewer,
                    RoleName = "Viewer",
                    Description = "Read-only report and dashboard access",
                    Permissions = "[\"view\"]",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };
        }
    }
}

