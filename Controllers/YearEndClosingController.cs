using AccuFlow.Models.YearEndClosing;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class YearEndClosingController : BaseController
    {
        private readonly IYearEndClosingService _yearEndClosingService;

        public YearEndClosingController(IYearEndClosingService yearEndClosingService) : base(yearEndClosingService)
        {
            _yearEndClosingService = yearEndClosingService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Year-End Closing";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Preview([FromBody] YearEndClosingRequest request) => Json(await _yearEndClosingService.Preview(request));

        [HttpPost]
        public async Task<IActionResult> Close([FromBody] YearEndClosingRequest request)
        {
            try
            {
                var closing = await _yearEndClosingService.Close(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = $"Fiscal year {request.FiscalYear} closed successfully", data = closing });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> History() => Json(await _yearEndClosingService.GetHistory());
    }
}