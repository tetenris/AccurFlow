using AccuFlow.Application.Features.Dashboard.Queries;
using AccuFlow.Models.Home;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly ISender _mediator;

        public HomeController(IBaseService baseService, ISender mediator) : base(baseService)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var summary = await _mediator.Send(new GetDashboardSummaryQuery());

            var model = new DashboardViewModel
            {
                FullName = _currentUserService?.FullName ?? "AccuFlow User",
                RoleName = _currentUserService?.RoleName ?? string.Empty,
                ActiveUsers = summary.ActiveUsers,
                ActiveRoles = summary.ActiveRoles,
                DraftJournals = summary.DraftJournals,
                PostedJournals = summary.PostedJournals,
                OpenInvoices = summary.OpenInvoices,
                OverdueInvoices = summary.OverdueInvoices,
                PendingPurchaseOrders = summary.PendingPurchaseOrders,
                Receivables = summary.Receivables,
                Payables = summary.Payables,
                CashMovementsThisMonth = summary.CashMovementsThisMonth,
                RecentActivities = summary.RecentActivities
                    .Select(x => new DashboardActivityItem
                    {
                        Title = x.Title,
                        Description = x.Description,
                        Category = x.Category,
                        Date = x.Date
                    })
                    .ToList()
            };

            return View(model);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}

