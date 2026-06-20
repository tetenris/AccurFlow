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
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<Services.Interfaces.IMenuService, MenuService>();
            services.AddScoped<IChartOfAccountService, ChartOfAccountService>();
            services.AddScoped<IJournalEntryService, JournalEntryService>();
            services.AddScoped<IGeneralLedgerService, GeneralLedgerService>();
            services.AddScoped<ITrialBalanceService, TrialBalanceService>();
            services.AddScoped<IFinancialStatementService, FinancialStatementService>();
            services.AddScoped<IRoleMenuService, RoleMenuService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IAgingReportService, AgingReportService>();
            services.AddScoped<IApprovalService, ApprovalService>();
            services.AddScoped<IDocumentAttachmentService, DocumentAttachmentService>();

            return services;
        }
    }
}
