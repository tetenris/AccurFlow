using AccuFlow.Application.Features.Invoices.Commands;
using AccuFlow.Application.Features.Invoices.Queries;
using AccuFlow.Models.Invoice;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class InvoiceController : BaseController
    {
        private readonly ISender _mediator;

        public InvoiceController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Invoice Management";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableInvoiceRequest request) => Json(await _mediator.Send(new GetInvoiceDatatableQuery(request)));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _mediator.Send(new GetInvoiceByIdQuery(id)));

        [HttpGet]
        public async Task<IActionResult> Print(Guid id)
        {
            var invoice = await _mediator.Send(new GetInvoiceByIdQuery(id));
            if (invoice == null) return NotFound();
            return View(invoice);
        }

        [HttpGet]
        public async Task<IActionResult> GetOpenInvoices(string invoiceType)
        {
            var result = await _mediator.Send(new GetInvoiceDatatableQuery(new DataTableInvoiceRequest
            {
                InvoiceType = invoiceType,
                Page = 1,
                Size = 1000
            }));

            return Json(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request)
        {
            try
            {
                await _mediator.Send(new CreateInvoiceCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Invoice created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateInvoiceRequest request)
        {
            try
            {
                await _mediator.Send(new UpdateInvoiceCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Invoice updated successfully" });
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
                await _mediator.Send(new DeleteInvoiceCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Invoice deleted successfully" });
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
                await _mediator.Send(new PostInvoiceCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Invoice posted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cancel([FromBody] Guid id)
        {
            try
            {
                await _mediator.Send(new CancelInvoiceCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Invoice cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}