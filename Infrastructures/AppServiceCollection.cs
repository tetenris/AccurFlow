using AccuFlow.Services;

namespace AccuFlow.Infrastructures
{
    public static class AppServiceCollection
    {
        public static IServiceCollection AddAppService(this IServiceCollection services, IConfiguration configuration)
        {
            // Register infrastructure services
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<PermissionAuthorizationFilter>();
            
            // Register base services
            services.AddScoped<IBaseService, BaseService>();
            
            // Register services
            services.AddScoped<Services.Interfaces.IMenuService, MenuService>();
            services.AddScoped<IChartOfAccountService, ChartOfAccountService>();
            services.AddScoped<IJournalEntryService, JournalEntryService>();
            services.AddScoped<IGeneralLedgerService, GeneralLedgerService>();
            services.AddScoped<ICashBankService, CashBankService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IItemGroupService, ItemGroupService>();
            services.AddScoped<IItemUnitService, ItemUnitService>();
            services.AddScoped<IStockOpnameService, StockOpnameService>();
            services.AddScoped<IStockTransferService, StockTransferService>();
            services.AddScoped<IStockBatchService, StockBatchService>();
            services.AddScoped<IPayrollService, PayrollService>();
            services.AddScoped<IProductionService, ProductionService>();
            services.AddScoped<IApprovalService, ApprovalService>();
            services.AddScoped<IDocumentAttachmentService, DocumentAttachmentService>();

            return services;
        }
    }
}
