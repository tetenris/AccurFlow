# Design Document

## Overview

Fitur database seeding untuk AccuFlow akan mengimplementasikan pola yang mirip dengan project KOMATSU, dengan struktur seeder yang modular dan mudah diperluas. Sistem ini akan menyediakan:

- Static seeder classes untuk setiap entity
- Central Seeder class yang mengkoordinasi semua seeding operations
- Controller untuk trigger seeding melalui UI atau API
- Safety mechanisms untuk mencegah data corruption

## Architecture

### Component Structure

```
AccuFlow/
├── Entities/
│   └── Seeders/
│       ├── Seeder.cs (Main coordinator)
│       ├── UserSeed.cs
│       ├── RoleSeed.cs
│       ├── ChartOfAccountSeed.cs
│       ├── AccountCategorySeed.cs
│       └── SampleTransactionSeed.cs (optional, dev only)
├── Controllers/
│   └── SeedController.cs
└── Views/
    └── Seed/
        └── Index.cshtml
```

### Design Pattern

Menggunakan **Static Factory Pattern** untuk seed data generation, mirip dengan implementasi KOMATSU:
- Setiap seeder memiliki static method `GetSeedData()` yang mengembalikan list of entities
- Main `Seeder` class memiliki static async methods untuk setiap entity type
- Seeder methods menggunakan `DbContext` untuk check existing data dan insert new data

## Components and Interfaces

### 1. Seeder.cs (Main Coordinator)

```csharp
namespace AccuFlow.Entities.Seeders
{
    public static class Seeder
    {
        public static async Task SeedUsers(AppDbContext dbContext)
        {
            var users = UserSeed.GetUserSeedData();
            var existingUsers = await dbContext.Set<UserEntity>().ToListAsync();
            var newUsers = users.Where(u => !existingUsers.Any(eu => eu.Email == u.Email)).ToList();
            
            if (newUsers.Any())
            {
                dbContext.Set<UserEntity>().AddRange(newUsers);
                await dbContext.SaveChangesAsync();
            }
        }

        public static async Task SeedRoles(AppDbContext dbContext)
        {
            var roles = RoleSeed.GetRoleSeedData();
            var existingRoles = await dbContext.Set<RoleEntity>().ToListAsync();
            var newRoles = roles.Where(r => !existingRoles.Any(er => er.RoleId == r.RoleId)).ToList();
            
            if (newRoles.Any())
            {
                dbContext.Set<RoleEntity>().AddRange(newRoles);
                await dbContext.SaveChangesAsync();
            }
        }

        public static async Task SeedChartOfAccounts(AppDbContext dbContext)
        {
            var accounts = ChartOfAccountSeed.GetChartOfAccountSeedData();
            var existingAccounts = await dbContext.Set<ChartOfAccountEntity>().ToListAsync();
            var newAccounts = accounts.Where(a => !existingAccounts.Any(ea => ea.AccountCode == a.AccountCode)).ToList();
            
            if (newAccounts.Any())
            {
                dbContext.Set<ChartOfAccountEntity>().AddRange(newAccounts);
                await dbContext.SaveChangesAsync();
            }
        }

        public static async Task SeedAll(AppDbContext dbContext)
        {
            await SeedRoles(dbContext);
            await SeedUsers(dbContext);
            await SeedChartOfAccounts(dbContext);
        }
    }
}
```

### 2. Individual Seed Classes

#### UserSeed.cs

```csharp
namespace AccuFlow.Entities.Seeders
{
    public static class UserSeed
    {
        public static List<UserEntity> GetUserSeedData()
        {
            return new List<UserEntity>
            {
                new UserEntity
                {
                    UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    Email = "admin@accuflow.com",
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    FullName = "System Administrator",
                    IsActive = true,
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000001")
                },
                new UserEntity
                {
                    UserId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    Email = "accountant@accuflow.com",
                    Username = "accountant",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Accountant123!"),
                    FullName = "Default Accountant",
                    IsActive = true,
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000002")
                }
            };
        }
    }
}
```

#### RoleSeed.cs

```csharp
namespace AccuFlow.Entities.Seeders
{
    public static class RoleSeed
    {
        public static List<RoleEntity> GetRoleSeedData()
        {
            return new List<RoleEntity>
            {
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    RoleName = "Administrator",
                    Description = "Full system access",
                    Permissions = "[\"all\"]"
                },
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    RoleName = "Accountant",
                    Description = "Accounting operations access",
                    Permissions = "[\"journal_entry\",\"invoice\",\"report\"]"
                },
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    RoleName = "Manager",
                    Description = "Management and approval access",
                    Permissions = "[\"view\",\"approve\"]"
                },
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                    RoleName = "User",
                    Description = "Basic user access",
                    Permissions = "[\"view\"]"
                }
            };
        }
    }
}
```

#### ChartOfAccountSeed.cs

```csharp
namespace AccuFlow.Entities.Seeders
{
    public static class ChartOfAccountSeed
    {
        public static List<ChartOfAccountEntity> GetChartOfAccountSeedData()
        {
            return new List<ChartOfAccountEntity>
            {
                // Assets
                new ChartOfAccountEntity
                {
                    AccountId = Guid.NewGuid(),
                    AccountCode = "1000",
                    AccountName = "Cash",
                    AccountType = "Asset",
                    ParentAccountId = null,
                    IsActive = true
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.NewGuid(),
                    AccountCode = "1100",
                    AccountName = "Accounts Receivable",
                    AccountType = "Asset",
                    ParentAccountId = null,
                    IsActive = true
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.NewGuid(),
                    AccountCode = "1200",
                    AccountName = "Inventory",
                    AccountType = "Asset",
                    ParentAccountId = null,
                    IsActive = true
                },
                
                // Liabilities
                new ChartOfAccountEntity
                {
                    AccountId = Guid.NewGuid(),
                    AccountCode = "2000",
                    AccountName = "Accounts Payable",
                    AccountType = "Liability",
                    ParentAccountId = null,
                    IsActive = true
                },
                
                // Equity
                new ChartOfAccountEntity
                {
                    AccountId = Guid.NewGuid(),
                    AccountCode = "3000",
                    AccountName = "Owner's Equity",
                    AccountType = "Equity",
                    ParentAccountId = null,
                    IsActive = true
                },
                
                // Revenue
                new ChartOfAccountEntity
                {
                    AccountId = Guid.NewGuid(),
                    AccountCode = "4000",
                    AccountName = "Sales Revenue",
                    AccountType = "Revenue",
                    ParentAccountId = null,
                    IsActive = true
                },
                
                // Expenses
                new ChartOfAccountEntity
                {
                    AccountId = Guid.NewGuid(),
                    AccountCode = "5000",
                    AccountName = "Cost of Goods Sold",
                    AccountType = "Expense",
                    ParentAccountId = null,
                    IsActive = true
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.NewGuid(),
                    AccountCode = "5100",
                    AccountName = "Operating Expenses",
                    AccountType = "Expense",
                    ParentAccountId = null,
                    IsActive = true
                }
            };
        }
    }
}
```

### 3. SeedController.cs

```csharp
namespace AccuFlow.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SeedController : BaseController
    {
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public SeedController(AppDbContext dbContext, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _environment = environment;
        }

        public IActionResult Index()
        {
            var model = new SeedViewModel
            {
                IsProduction = _environment.IsProduction(),
                AvailableSeeders = new List<string>
                {
                    "Roles",
                    "Users",
                    "ChartOfAccounts",
                    "All"
                }
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RunSeeder(string seederName, bool confirmed = false)
        {
            try
            {
                if (_environment.IsProduction() && !confirmed)
                {
                    return Json(new { success = false, message = "Production environment requires confirmation" });
                }

                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                
                try
                {
                    switch (seederName.ToLower())
                    {
                        case "roles":
                            await Seeder.SeedRoles(_dbContext);
                            break;
                        case "users":
                            await Seeder.SeedUsers(_dbContext);
                            break;
                        case "chartofaccounts":
                            await Seeder.SeedChartOfAccounts(_dbContext);
                            break;
                        case "all":
                            await Seeder.SeedAll(_dbContext);
                            break;
                        default:
                            return Json(new { success = false, message = "Invalid seeder name" });
                    }

                    await transaction.CommitAsync();
                    return Json(new { success = true, message = $"{seederName} seeded successfully" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = $"Seeding failed: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
```

### 4. View Model

```csharp
namespace AccuFlow.Models.Seed
{
    public class SeedViewModel
    {
        public bool IsProduction { get; set; }
        public List<string> AvailableSeeders { get; set; }
        public string Message { get; set; }
    }
}
```

## Data Models

### Existing Entities
Menggunakan entities yang sudah ada:
- `UserEntity` - untuk user seeding
- `RoleEntity` - untuk role seeding (jika sudah ada)
- `ChartOfAccountEntity` - untuk chart of accounts seeding

### New Entities (if needed)
Jika belum ada, perlu dibuat:
- `RoleEntity` dengan properties: RoleId, RoleName, Description, Permissions
- Relationship antara `UserEntity` dan `RoleEntity`

## Error Handling

### Duplicate Detection
- Check existing data menggunakan unique identifiers (Email untuk User, AccountCode untuk ChartOfAccount)
- Skip insertion jika data sudah ada
- Log warning untuk skipped records

### Transaction Management
- Wrap seeding operations dalam database transaction
- Rollback on any error
- Commit only when all operations succeed

### Validation
- Validate required fields before insertion
- Validate format (email, account codes)
- Validate foreign key relationships

### Logging
```csharp
public static async Task SeedUsers(AppDbContext dbContext, ILogger logger)
{
    try
    {
        logger.LogInformation("Starting user seeding...");
        var users = UserSeed.GetUserSeedData();
        var existingUsers = await dbContext.Set<UserEntity>().ToListAsync();
        var newUsers = users.Where(u => !existingUsers.Any(eu => eu.Email == u.Email)).ToList();
        
        if (newUsers.Any())
        {
            dbContext.Set<UserEntity>().AddRange(newUsers);
            await dbContext.SaveChangesAsync();
            logger.LogInformation($"Seeded {newUsers.Count} users successfully");
        }
        else
        {
            logger.LogInformation("No new users to seed");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error seeding users");
        throw;
    }
}
```

## Testing Strategy

### Unit Tests
- Test each seeder's `GetSeedData()` method returns valid data
- Test duplicate detection logic
- Test validation logic

### Integration Tests
- Test seeding with empty database
- Test seeding with existing data (should skip duplicates)
- Test transaction rollback on error
- Test seeding in different environments

### Manual Testing
- Test through UI in development environment
- Test production safety confirmation
- Test error messages and user feedback
- Verify data integrity after seeding

## Security Considerations

### Authorization
- Only Administrator role can access seed functionality
- Add `[Authorize(Roles = "Administrator")]` attribute to controller

### Environment Protection
- Require additional confirmation in Production
- Consider disabling seeding in Production entirely
- Log all seeding operations with user and timestamp

### Data Safety
- Use transactions to prevent partial data insertion
- Validate all data before insertion
- Never delete existing data during seeding

## UI Design

### Seed Index Page
- Display list of available seeders with descriptions
- Show environment indicator (Development/Production)
- Provide individual seeder buttons and "Seed All" button
- Show confirmation dialog for Production environment
- Display results/messages after seeding operation

### Features
- Progress indicator during seeding
- Success/error messages
- List of seeded records count
- Option to view seeded data
