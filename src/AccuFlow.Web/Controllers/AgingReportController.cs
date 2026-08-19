using AccuFlow.Application.Features.AgingReports.Queries;
using AccuFlow.Models.AgingReport;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class AgingReportController : BaseController
    {
        private readonly ISender _mediator;

        public AgingReportController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Aging Report";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Generate([FromBody] AgingReportRequest request) => Json(await _mediator.Send(new GetAgingReportQuery(request)));
    }
}