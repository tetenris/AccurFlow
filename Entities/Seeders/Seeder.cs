using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AccuFlow.Entities.Seeders
{
    public static class Seeder
    {
        public static async Task SeedRoles(AppDbContext dbContext, ILogger logger)
        {
            logger.LogInformation("Seeding Roles...");
            var roles = RoleSeed.GetRoleSeedData();
            var existingRoles = await dbContext.Roles.ToListAsync();
            var newRoles = roles.Where(r => !existingRoles.Any(er => er.RoleId == r.RoleId)).ToList();
            var updatedRoles = 0;

            foreach (var seedRole in roles)
            {
                var existingRole = existingRoles.FirstOrDefault(r => r.RoleId == seedRole.RoleId);
                if (existingRole == null)
                {
                    continue;
                }

                existingRole.RoleType = seedRole.RoleType;
                existingRole.RoleName = seedRole.RoleName;
                existingRole.Description = seedRole.Description;
                existingRole.Permissions = seedRole.Permissions;
                existingRole.IsActive = true;
                existingRole.IsDeleted = false;
                existingRole.DeletedAt = null;
                existingRole.DeletedBy = null;
                updatedRoles++;
            }
            
            if (newRoles.Any())
            {
                dbContext.Roles.AddRange(newRoles);
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Seeded {newRoles.Count} roles");
            }
            else if (updatedRoles > 0)
            {
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Synced {updatedRoles} roles");
            }
            else
            {
                logger.LogInformation("No new roles to seed");
            }
        }

        public static async Task SeedUsers(AppDbContext dbContext, ILogger logger)
        {
            logger.LogInformation("Seeding Users...");
            var users = UserSeed.GetUserSeedData();
            var existingUsers = await dbContext.Users.ToListAsync();
            var newUsers = users.Where(u => !existingUsers.Any(eu => eu.UserId == u.UserId)).ToList();
            var updatedUsers = 0;

            foreach (var seedUser in users)
            {
                var existingUser = existingUsers.FirstOrDefault(u => u.UserId == seedUser.UserId);
                if (existingUser == null)
                {
                    continue;
                }

                existingUser.UserName = seedUser.UserName;
                existingUser.Email = seedUser.Email;
                existingUser.FullName = seedUser.FullName;
                existingUser.RoleId = seedUser.RoleId;
                existingUser.IsActive = true;
                existingUser.IsDeleted = false;
                existingUser.DeletedAt = null;
                existingUser.DeletedBy = null;
                if (string.IsNullOrWhiteSpace(existingUser.PasswordHash))
                {
                    existingUser.PasswordHash = seedUser.PasswordHash;
                }
                updatedUsers++;
            }
            
            if (newUsers.Any())
            {
                dbContext.Users.AddRange(newUsers);
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Seeded {newUsers.Count} users");
            }
            else if (updatedUsers > 0)
            {
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Synced {updatedUsers} users");
            }
            else
            {
                logger.LogInformation("No new users to seed");
            }
        }

        public static async Task SeedChartOfAccounts(AppDbContext dbContext, ILogger logger)
        {
            logger.LogInformation("Seeding Chart of Accounts...");
            var accounts = ChartOfAccountSeed.GetChartOfAccountSeedData();
            var existingAccountIds = await dbContext.ChartOfAccounts
                .AsNoTracking()
                .Select(a => a.AccountId)
                .ToListAsync();
            
            var newAccounts = accounts.Where(a => !existingAccountIds.Contains(a.AccountId)).ToList();
            
            if (newAccounts.Any())
            {
                // Add accounts one by one to avoid tracking issues
                foreach (var account in newAccounts.OrderBy(a => a.Level))
                {
                    dbContext.ChartOfAccounts.Add(account);
                }
                await dbContext.SaveChangesAsync();
                
                logger.LogInformation($"Seeded {newAccounts.Count} chart of accounts");
            }
            else
            {
                logger.LogInformation("No new chart of accounts to seed");
            }
        }

        public static async Task SeedCustomers(AppDbContext dbContext, ILogger logger)
        {
            logger.LogInformation("Seeding Customers...");
            var customers = CustomerSeed.GetCustomerSeedData();
            var existingCustomers = await dbContext.Customers.ToListAsync();
            var newCustomers = customers.Where(c => !existingCustomers.Any(ec => ec.CustomerId == c.CustomerId)).ToList();
            
            if (newCustomers.Any())
            {
                dbContext.Customers.AddRange(newCustomers);
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Seeded {newCustomers.Count} customers");
            }
            else
            {
                logger.LogInformation("No new customers to seed");
            }
        }

        public static async Task SeedSuppliers(AppDbContext dbContext, ILogger logger)
        {
            logger.LogInformation("Seeding Suppliers...");
            var suppliers = SupplierSeed.GetSupplierSeedData();
            var existingSuppliers = await dbContext.Suppliers.ToListAsync();
            var newSuppliers = suppliers.Where(s => !existingSuppliers.Any(es => es.SupplierId == s.SupplierId)).ToList();

            if (newSuppliers.Any())
            {
                dbContext.Suppliers.AddRange(newSuppliers);
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Seeded {newSuppliers.Count} suppliers");
            }
            else
            {
                logger.LogInformation("No new suppliers to seed");
            }
        }

        public static async Task SeedBusinessModules(AppDbContext dbContext, ILogger logger)
        {
            logger.LogInformation("Seeding business module master data...");

            var warehouses = BusinessModuleSeed.GetWarehouseSeedData();
            var existingWarehouseIds = await dbContext.Warehouses.Select(x => x.WarehouseId).ToListAsync();
            var newWarehouses = warehouses.Where(x => !existingWarehouseIds.Contains(x.WarehouseId)).ToList();
            if (newWarehouses.Any())
            {
                dbContext.Warehouses.AddRange(newWarehouses);
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Seeded {newWarehouses.Count} warehouses");
            }

            var taxes = BusinessModuleSeed.GetTaxSeedData();
            var existingTaxIds = await dbContext.Taxes.Select(x => x.TaxId).ToListAsync();
            var newTaxes = taxes.Where(x => !existingTaxIds.Contains(x.TaxId)).ToList();
            if (newTaxes.Any())
            {
                dbContext.Taxes.AddRange(newTaxes);
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Seeded {newTaxes.Count} taxes");
            }
        }

        public static async Task SeedSampleTransactions(AppDbContext dbContext, ILogger logger)
        {
            logger.LogInformation("Seeding Sample Transactions...");
            // Implementation for sample transactions
            logger.LogInformation("Sample transactions seeding completed");
        }

        public static async Task SeedMenu(AppDbContext dbContext, ILogger logger)
        {
            logger.LogInformation("Seeding Menus...");
            var menus = MenuSeed.GetMenuSeedData();
            var existingMenus = await dbContext.Menus.ToListAsync();
            var newMenus = menus.Where(m => !existingMenus.Any(em => em.MenuId == m.MenuId)).ToList();
            
            if (newMenus.Any())
            {
                dbContext.Menus.AddRange(newMenus);
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Seeded {newMenus.Count} menus");
            }
            else
            {
                logger.LogInformation("No new menus to seed");
            }

            var updatedMenus = 0;
            foreach (var seedMenu in menus)
            {
                var existingMenu = existingMenus.FirstOrDefault(m => m.MenuId == seedMenu.MenuId);
                if (existingMenu == null)
                {
                    continue;
                }

                existingMenu.MenuParentId = seedMenu.MenuParentId;
                existingMenu.Icon = seedMenu.Icon;
                existingMenu.Name = seedMenu.Name;
                existingMenu.Controller = seedMenu.Controller;
                existingMenu.Action = seedMenu.Action;
                existingMenu.Sequence = seedMenu.Sequence;
                existingMenu.IsDeleted = false;
                existingMenu.DeletedAt = null;
                existingMenu.DeletedBy = null;
                updatedMenus++;
            }

            if (updatedMenus > 0)
            {
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Updated {updatedMenus} existing menus");
            }

        }

        public static async Task SeedRoleMenus(AppDbContext dbContext, ILogger logger)
        {
            logger.LogInformation("Seeding Role Menu permissions...");

            var systemRoleIds = new[]
            {
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Guid.Parse("00000000-0000-0000-0000-000000000002")
            };
            var adminPermissions = RoleMenuSeed.GetDefaultAdminPermissions();
            var existingPermissions = await dbContext.RoleMenus
                .Where(x => systemRoleIds.Contains(x.RoleId))
                .ToListAsync();

            var updatedCount = 0;
            foreach (var roleId in systemRoleIds)
            {
                foreach (var permission in adminPermissions)
                {
                    var existing = existingPermissions.FirstOrDefault(x => x.RoleId == roleId && x.MenuId == permission.MenuId);
                    if (existing == null)
                    {
                        dbContext.RoleMenus.Add(new RoleMenuEntity
                        {
                            RoleMenuId = Guid.NewGuid(),
                            RoleId = roleId,
                            MenuId = permission.MenuId,
                            CanView = true,
                            CanAdd = true,
                            CanEdit = true,
                            CanDelete = true,
                            CanPost = true,
                            CanReverse = true,
                            CreatedBy = "System",
                            CreatedAt = DateTime.UtcNow
                        });
                        updatedCount++;
                        continue;
                    }

                    existing.CanView = true;
                    existing.CanAdd = true;
                    existing.CanEdit = true;
                    existing.CanDelete = true;
                    existing.CanPost = true;
                    existing.CanReverse = true;
                    existing.IsDeleted = false;
                    existing.DeletedAt = null;
                    existing.DeletedBy = null;
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Synced {updatedCount} admin role menu permissions");
            }
        }

        public static async Task SeedAll(AppDbContext dbContext, ILogger logger, string environmentName)
        {
            logger.LogInformation($"Starting SeedAll in {environmentName} environment...");
            
            await SeedRoles(dbContext, logger);
            await SeedUsers(dbContext, logger);
            await SeedChartOfAccounts(dbContext, logger);
            await SeedCustomers(dbContext, logger);
            await SeedSuppliers(dbContext, logger);
            await SeedBusinessModules(dbContext, logger);
            await SeedMenu(dbContext, logger);
            await SeedRoleMenus(dbContext, logger);
            
            // Only seed sample transactions in Development or Staging
            if (environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase) || 
                environmentName.Equals("Staging", StringComparison.OrdinalIgnoreCase))
            {
                await SeedSampleTransactions(dbContext, logger);
            }
            
            logger.LogInformation("SeedAll completed successfully");
        }
    }
}
