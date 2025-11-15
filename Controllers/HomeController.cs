using AccuFlow.Services;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IBaseService baseService) : base(baseService)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
