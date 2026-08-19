using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Infrastructures;
using AccuFlow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AccuFlow.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IBaseService _baseService;
        protected ICurrentUserService? _currentUserService => HttpContext?.RequestServices?.GetService<ICurrentUserService>();

        public BaseController(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Set user info to ViewBag/TempData only if user is authenticated
            if (User?.Identity?.IsAuthenticated == true && _currentUserService != null)
            {
                TempData["UserId"] = _currentUserService.UserId;
                TempData["FullName"] = _currentUserService.FullName;
                TempData["RoleName"] = _currentUserService.RoleName;
                ViewBag.UserName = _currentUserService.UserName;
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
