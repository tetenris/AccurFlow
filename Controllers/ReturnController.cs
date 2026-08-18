using AccuFlow.Application.Features.Returns.Commands;
using AccuFlow.Application.Features.Returns.Queries;
using AccuFlow.Models.Return;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class ReturnController : BaseController
    {
        private readonly ISender _mediator;

        public ReturnController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Sales & Purchase Returns";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableReturnRequest request) => Json(await _mediator.Send(new GetReturnDatatableQuery(request)));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _mediator.Send(new GetReturnByIdQuery(id)));

        [HttpGet]
        public async Task<IActionResult> GetWarehouses() => Json(await _mediator.Send(new GetReturnWarehousesQuery()));

        [HttpGet]
        public async Task<IActionResult> GetInvoices(string returnType) => Json(await _mediator.Send(new GetReturnInvoicesQuery(returnType)));

        [HttpGet]
        public async Task<IActionResult> GetInvoiceLines(Guid invoiceId) => Json(await _mediator.Send(new GetReturnInvoiceLinesQuery(invoiceId)));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReturnRequest request)
        {
            try
            {
                await _mediator.Send(new CreateReturnCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Return created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateReturnRequest request)
        {
            try
            {
                await _mediator.Send(new UpdateReturnCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Return updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Guid id)
        {
            try
            {
                await _mediator.Send(new PostReturnCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Return posted successfully" });
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
                await _mediator.Send(new DeleteReturnCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Return deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}