using AccuFlow.Infrastructure.Persistence;
using AccuFlow.Models.Home;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly AppDbContext _dbContext;

        public HomeController(IBaseService baseService, AppDbContext dbContext) : base(baseService)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var salesInvoices = _dbContext.Invoices
                .Where(x => !x.IsDeleted && x.InvoiceType == "Sales" && x.Status != "Cancelled");
            var purchaseInvoices = _dbContext.Invoices
                .Where(x => !x.IsDeleted && x.InvoiceType == "Purchase" && x.Status != "Cancelled");

            var model = new DashboardViewModel
            {
                FullName = _currentUserService?.FullName ?? "AccuFlow User",
                RoleName = _currentUserService?.RoleName ?? string.Empty,
                ActiveUsers = await _dbContext.Users.CountAsync(x => !x.IsDeleted && x.IsActive),
                ActiveRoles = await _dbContext.Roles.CountAsync(x => !x.IsDeleted && x.IsActive),
                DraftJournals = await _dbContext.JournalEntries.CountAsync(x => !x.IsDeleted && x.Status == "Draft"),
                PostedJournals = await _dbContext.JournalEntries.CountAsync(x => !x.IsDeleted && x.Status == "Posted"),
                OpenInvoices = await salesInvoices.CountAsync(x => x.TotalAmount > x.PaidAmount),
                OverdueInvoices = await salesInvoices.CountAsync(x => x.TotalAmount > x.PaidAmount && x.DueDate.Date < today),
                PendingPurchaseOrders = await _dbContext.PurchaseOrders.CountAsync(x => !x.IsDeleted && x.Status != "Completed" && x.Status != "Cancelled"),
                Receivables = await salesInvoices.SumAsync(x => x.TotalAmount - x.PaidAmount),
                Payables = await purchaseInvoices.SumAsync(x => x.TotalAmount - x.PaidAmount),
                CashMovementsThisMonth = await _dbContext.Payments
                    .Where(x => !x.IsDeleted && x.Status != "Cancelled" && x.PaymentDate >= monthStart)
                    .SumAsync(x => x.TotalAmount)
            };

            var recentJournals = await _dbContext.JournalEntries
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Take(4)
                .Select(x => new DashboardActivityItem
                {
                    Title = x.JournalNumber,
                    Description = x.Description,
                    Category = "Journal",
                    Date = x.CreatedAt
                })
                .ToListAsync();

            var recentInvoices = await _dbContext.Invoices
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Take(4)
                .Select(x => new DashboardActivityItem
                {
                    Title = x.InvoiceNumber,
                    Description = x.InvoiceType + " invoice - " + x.Status,
                    Category = "Invoice",
                    Date = x.CreatedAt
                })
                .ToListAsync();

            model.RecentActivities = recentJournals
                .Concat(recentInvoices)
                .OrderByDescending(x => x.Date)
                .Take(6)
                .ToList();

            return View(model);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}

