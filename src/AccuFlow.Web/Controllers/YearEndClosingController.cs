using AccuFlow.Application.Features.YearEndClosings.Commands;
using AccuFlow.Application.Features.YearEndClosings.Queries;
using AccuFlow.Models.YearEndClosing;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class YearEndClosingController : BaseController
    {
        private readonly ISender _mediator;

        public YearEndClosingController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Year-End Closing";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Preview([FromBody] YearEndClosingRequest request) => Json(await _mediator.Send(new GetYearEndClosingPreviewQuery(request)));

        [HttpPost]
        public async Task<IActionResult> Close([FromBody] YearEndClosingRequest request)
        {
            try
            {
                var closing = await _mediator.Send(new CloseYearEndClosingCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = $"Fiscal year {request.FiscalYear} closed successfully", data = closing });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> History() => Json(await _mediator.Send(new GetYearEndClosingHistoryQuery()));
    }
}