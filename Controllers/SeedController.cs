using AccuFlow.Entities.Context;
using AccuFlow.Entities.Seeders;
using AccuFlow.Models.Seed;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SeedController : BaseController
    {
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<SeedController> _logger;

        public SeedController(AppDbContext dbContext, IWebHostEnvironment environment, ILogger<SeedController> logger, IBaseService baseService) : base(baseService)
        {
            _dbContext = dbContext;
            _environment = environment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var availableSeeders = new List<string>
            {
                "Roles",
                "Users",
                "ChartOfAccounts",
                "Customers",
                "Suppliers",
                "BusinessModules",
                "Menu",
                "RoleMenus"
            };
            
            // Add sample transactions seeder only in Development or Staging
            if (_environment.IsDevelopment() || _environment.IsStaging())
            {
                availableSeeders.Add("SampleTransactions");
            }
            
            availableSeeders.Add("All");
            
            var model = new SeedViewModel
            {
                IsProduction = _environment.IsProduction(),
                AvailableSeeders = availableSeeders
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RunSeeder(string seederName, bool confirmed = false)
        {
            try
            {
                _logger.LogInformation($"Seeding request received for: {seederName}");
                
                // Production environment requires confirmation
                if (_environment.IsProduction() && !confirmed)
                {
                    _logger.LogWarning($"Seeding attempt in production without confirmation for: {seederName}");
                    return Json(new { success = false, message = "Production environment requires confirmation" });
                }

                // Begin transaction for data consistency
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                
                try
                {
                    // Get current environment name
                    var environmentName = _environment.EnvironmentName;
                    
                    // Execute the appropriate seeder based on the name
                    switch (seederName.ToLower())
                    {
                        case "roles":
                            await Seeder.SeedRoles(_dbContext, _logger);
                            break;
                        case "users":
                            await Seeder.SeedUsers(_dbContext, _logger);
                            break;
                        case "chartofaccounts":
                            await Seeder.SeedChartOfAccounts(_dbContext, _logger);
                            break;
                        case "customers":
                            await Seeder.SeedCustomers(_dbContext, _logger);
                            break;
                        case "suppliers":
                            await Seeder.SeedSuppliers(_dbContext, _logger);
                            break;
                        case "businessmodules":
                            await Seeder.SeedBusinessModules(_dbContext, _logger);
                            break;
                        case "menu":
                            await Seeder.SeedMenu(_dbContext, _logger);
                            break;
                        case "rolemenus":
                            await Seeder.SeedRoleMenus(_dbContext, _logger);
                            break;
                        case "sampletransactions":
                            // Only allow in Development or Staging
                            if (environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase) || 
                                environmentName.Equals("Staging", StringComparison.OrdinalIgnoreCase))
                            {
                                await Seeder.SeedSampleTransactions(_dbContext, _logger);
                            }
                            else
                            {
                                _logger.LogWarning($"Sample transaction seeding not allowed in {environmentName} environment");
                                return Json(new { success = false, message = $"Sample transactions can only be seeded in Development or Staging environments" });
                            }
                            break;
                        case "all":
                            await Seeder.SeedAll(_dbContext, _logger, environmentName);
                            break;
                        default:
                            _logger.LogWarning($"Invalid seeder name requested: {seederName}");
                            return Json(new { success = false, message = "Invalid seeder name" });
                    }

                    // Commit transaction if all operations succeed
                    await transaction.CommitAsync();
                    _logger.LogInformation($"Successfully completed seeding for: {seederName}");
                    return Json(new { success = true, message = $"{seederName} seeded successfully" });
                }
                catch (Exception ex)
                {
                    // Rollback transaction on error
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"Transaction rolled back for seeder: {seederName}");
                    return Json(new { success = false, message = $"Seeding failed: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in RunSeeder for: {seederName}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
