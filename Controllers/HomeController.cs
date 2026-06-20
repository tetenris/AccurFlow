using AccuFlow.Models.Home;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        public HomeController(IBaseService baseService) : base(baseService)
        {
        }

        public IActionResult Index()
        {
            var model = new DashboardViewModel
            {
                FullName = _currentUserService?.FullName ?? "AccuFlow User",
                RoleName = _currentUserService?.RoleName ?? string.Empty
            };

            return View(model);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
