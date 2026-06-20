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
            
            if (newRoles.Any())
            {
                dbContext.Roles.AddRange(newRoles);
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Seeded {newRoles.Count} roles");
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
            
            if (newUsers.Any())
            {
                dbContext.Users.AddRange(newUsers);
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Seeded {newUsers.Count} users");
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

            var disabledMenuIds = new[]
            {
                Guid.Parse("00000000-0000-0000-0000-000000000009")
            };

            var menusToDisable = await dbContext.Menus
                .Where(m => disabledMenuIds.Contains(m.MenuId) && !m.IsDeleted)
                .ToListAsync();

            if (menusToDisable.Any())
            {
                foreach (var menu in menusToDisable)
                {
                    menu.IsDeleted = true;
                    menu.DeletedAt = DateTime.UtcNow;
                    menu.DeletedBy = "system";
                }

                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Disabled {menusToDisable.Count} unavailable menus");
            }
        }

        public static async Task SeedAll(AppDbContext dbContext, ILogger logger, string environmentName)
        {
            logger.LogInformation($"Starting SeedAll in {environmentName} environment...");
            
            await SeedRoles(dbContext, logger);
            await SeedUsers(dbContext, logger);
            await SeedChartOfAccounts(dbContext, logger);
            await SeedCustomers(dbContext, logger);
            await SeedMenu(dbContext, logger);
            
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
