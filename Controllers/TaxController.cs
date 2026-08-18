using AccuFlow.Application.Features.Taxes.Commands;
using AccuFlow.Application.Features.Taxes.Queries;
using AccuFlow.Models.Tax;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class TaxController : BaseController
    {
        private readonly ISender _mediator;

        public TaxController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Taxes";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableTaxRequest request) => Json(await _mediator.Send(new GetTaxDatatableQuery(request)));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _mediator.Send(new GetTaxByIdQuery(id)));

        [HttpPost]
        public async Task<IActionResult> VatReport([FromBody] VatReportRequest request) => Json(await _mediator.Send(new GetVatReportQuery(request)));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaxRequest request)
        {
            try
            {
                await _mediator.Send(new CreateTaxCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Tax created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateTaxRequest request)
        {
            try
            {
                await _mediator.Send(new UpdateTaxCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Tax updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] Guid id)
        {
            try
            {
                await _mediator.Send(new DeleteTaxCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Tax deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}