# ============================================
# AccuFlow Project Generator
# Based on KMSI.SuperApps.Ews Template
# ============================================

param(
    [string]$TargetPath = "D:\ASP.NET\2025\AKURAT\AccuFlow",
    [string]$ProjectName = "AccuFlow",
    [string]$Namespace = "AccuFlow"
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  AccuFlow Project Generator" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$SourcePath = $PSScriptRoot

# Create target directory
Write-Host "Creating project directory: $TargetPath" -ForegroundColor Yellow
New-Item -ItemType Directory -Path $TargetPath -Force | Out-Null

# Define folder structure
$folders = @(
    "Controllers",
    "Models/Account",
    "Models/BaseModel", 
    "Models/ChartOfAccount",
    "Models/JournalEntry",
    "Models/Invoice",
    "Models/PurchaseOrder",
    "Models/Inventory",
    "Models/Report",
    "Services",
    "Entities/Abstractions",
    "Entities/Context",
    "Entities/Entity",
    "Entities/EntityConfigurations",
    "Entities/Enums",
    "Entities/Migrations",
    "Entities/Seeders",
    "Views/Shared",
    "Views/Home",
    "Views/Account",
    "Views/ChartOfAccount",
    "Views/JournalEntry",
    "Views/Invoice",
    "Views/PurchaseOrder",
    "Views/Inventory",
    "Views/Report",
    "wwwroot/assets",
    "wwwroot/css",
    "wwwroot/js",
    "wwwroot/custom",
    "wwwroot/lib",
    "wwwroot/theme",
    "Extentions",
    "Helpers",
    "Infrastructures",
    "Job",
    "Properties",
    "Database"
)

Write-Host "Creating folder structure..." -ForegroundColor Yellow
foreach ($folder in $folders) {
    $fullPath = Join-Path $TargetPath $folder
    New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
    Write-Host "  Created: $folder" -ForegroundColor Green
}

Write-Host ""
Write-Host "Generating project files..." -ForegroundColor Yellow

# Generate .csproj file
$csprojContent = @"
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UserSecretsId>$(New-Guid)</UserSecretsId>
    <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="DinkToPdf" Version="1.0.8" />
    <PackageReference Include="Hangfire" Version="1.8.21" />
    <PackageReference Include="Hangfire.Dashboard.BasicAuthorization" Version="1.0.0" />
    <PackageReference Include="Hangfire.SqlServer" Version="1.8.21" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.1" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.1">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.1" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.1">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="NPOI" Version="2.7.4" />
  </ItemGroup>

</Project>
"@

$csprojContent | Out-File -FilePath (Join-Path $TargetPath "$ProjectName.csproj") -Encoding UTF8
Write-Host "  Generated: $ProjectName.csproj" -ForegroundColor Green

# Generate appsettings.json
$appsettingsContent = @"
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=AccuFlowDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
"@

$appsettingsContent | Out-File -FilePath (Join-Path $TargetPath "appsettings.json") -Encoding UTF8
Write-Host "  Generated: appsettings.json" -ForegroundColor Green

# Generate appsettings.Development.json
$appsettingsDevContent = @"
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
"@

$appsettingsDevContent | Out-File -FilePath (Join-Path $TargetPath "appsettings.Development.json") -Encoding UTF8
Write-Host "  Generated: appsettings.Development.json" -ForegroundColor Green

Write-Host ""
Write-Host "Copying wwwroot (Metronic template)..." -ForegroundColor Yellow
Write-Host "  This may take a while..." -ForegroundColor Gray

# Copy wwwroot selectively (exclude large unnecessary files)
$wwwrootSource = Join-Path $SourcePath "wwwroot"
$wwwrootTarget = Join-Path $TargetPath "wwwroot"

if (Test-Path $wwwrootSource) {
    # Copy essential folders
    $essentialFolders = @("assets", "css", "js", "lib", "theme")
    foreach ($folder in $essentialFolders) {
        $src = Join-Path $wwwrootSource $folder
        $dst = Join-Path $wwwrootTarget $folder
        if (Test-Path $src) {
            Copy-Item -Path $src -Destination $dst -Recurse -Force
            Write-Host "  Copied: wwwroot/$folder" -ForegroundColor Green
        }
    }
    
    # Copy favicon
    $favicon = Join-Path $wwwrootSource "favicon.ico"
    if (Test-Path $favicon) {
        Copy-Item -Path $favicon -Destination $wwwrootTarget -Force
        Write-Host "  Copied: favicon.ico" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Project Generation Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Open the project in Visual Studio or VS Code" -ForegroundColor White
Write-Host "2. Update connection string in appsettings.json" -ForegroundColor White
Write-Host "3. Run: dotnet restore" -ForegroundColor White
Write-Host "4. Run: dotnet ef migrations add InitialCreate" -ForegroundColor White
Write-Host "5. Run: dotnet ef database update" -ForegroundColor White
Write-Host "6. Run: dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "Project location: $TargetPath" -ForegroundColor Cyan
Write-Host ""

# ============================================
# Generate Core Files
# ============================================

Write-Host "Generating core application files..." -ForegroundColor Yellow

# Program.cs
$programCs = @"
using Hangfire;
using $Namespace.Entities.Context;
using $Namespace.Extentions;
using $Namespace.Infrastructures;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
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

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

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
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    AppDbContext appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await appDbContext.Database.MigrateAsync();
}

app.UseHangfireDashboardWithAuth(builder.Configuration);

app.Run();
"@

$programCs | Out-File -FilePath (Join-Path $TargetPath "Program.cs") -Encoding UTF8
Write-Host "  Generated: Program.cs" -ForegroundColor Green

Write-Host ""
Write-Host "To complete the setup, run the following script:" -ForegroundColor Cyan
Write-Host "  .\GenerateAccuFlowFiles.ps1" -ForegroundColor White
Write-Host ""
Write-Host "This will generate all Controllers, Services, Models, and Views" -ForegroundColor Gray
