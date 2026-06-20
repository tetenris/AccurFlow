using AccuFlow.Models.AgingReport;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class AgingReportController : BaseController
    {
        private readonly IAgingReportService _agingReportService;

        public AgingReportController(IAgingReportService agingReportService) : base(agingReportService)
        {
            _agingReportService = agingReportService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Aging Report";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Generate([FromBody] AgingReportRequest request) => Json(await _agingReportService.GetAging(request));
    }
}
