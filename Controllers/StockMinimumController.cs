using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class StockMinimumController : BaseController
    {
        public StockMinimumController(IBaseService baseService) : base(baseService)
        {
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Stock Minimum";
            return View();
        }
    }
}