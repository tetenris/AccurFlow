using AccuFlow.Application.Features.SalesQuotations.Commands;
using AccuFlow.Application.Features.SalesQuotations.Queries;
using AccuFlow.Models.Quotation;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class SalesQuotationController : BaseController
    {
        private readonly ISender _mediator;

        public SalesQuotationController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Sales Quotation";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableQuotationRequest request) => Json(await _mediator.Send(new GetQuotationDatatableQuery(request)));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _mediator.Send(new GetQuotationByIdQuery(id)));

        [HttpGet]
        public async Task<IActionResult> GetApprovedQuotes() => Json(await _mediator.Send(new GetApprovedQuotesQuery()));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQuotationRequest request)
        {
            try
            {
                await _mediator.Send(new CreateQuotationCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Quotation created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateQuotationRequest request)
        {
            try
            {
                await _mediator.Send(new UpdateQuotationCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Quotation updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Approve([FromBody] Guid id)
        {
            try
            {
                await _mediator.Send(new ApproveQuotationCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Quotation approved successfully" });
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
                await _mediator.Send(new DeleteQuotationCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Quotation deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}