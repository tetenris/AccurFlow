using Hangfire;
using AccuFlow.Entities.Context;
using AccuFlow.Extentions;
using AccuFlow.Infrastructures;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<PermissionAuthorizationFilter>();
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
    });

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache();
builder.Services.AddAppService(builder.Configuration);
builder.Services.AddHangfireServices(builder.Configuration);

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(1800);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddRazorPages(); // .AddRazorRuntimeCompilation() requires Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation package

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    AppDbContext appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await appDbContext.Database.MigrateAsync();
    
    // Auto-seed essential data
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await AccuFlow.Entities.Seeders.Seeder.SeedRoles(appDbContext, logger);
    await AccuFlow.Entities.Seeders.Seeder.SeedUsers(appDbContext, logger);
    await AccuFlow.Entities.Seeders.Seeder.SeedChartOfAccounts(appDbContext, logger);
    await AccuFlow.Entities.Seeders.Seeder.SeedCustomers(appDbContext, logger);
    await AccuFlow.Entities.Seeders.Seeder.SeedSuppliers(appDbContext, logger);
    await AccuFlow.Entities.Seeders.Seeder.SeedBusinessModules(appDbContext, logger);
    await AccuFlow.Entities.Seeders.Seeder.SeedMenu(appDbContext, logger);
    await AccuFlow.Entities.Seeders.Seeder.SeedRoleMenus(appDbContext, logger);
}

app.UseHangfireDashboardWithAuth(builder.Configuration);

app.Run();
