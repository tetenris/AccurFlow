using AccuFlow.Application.Features.Payments.Commands;
using AccuFlow.Application.Features.Payments.Queries;
using AccuFlow.Models.Payment;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class PaymentController : BaseController
    {
        private readonly ISender _mediator;

        public PaymentController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Payment & Receipt";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTablePaymentRequest request) => Json(await _mediator.Send(new GetPaymentDatatableQuery(request)));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _mediator.Send(new GetPaymentByIdQuery(id)));

        [HttpGet]
        public async Task<IActionResult> Print(Guid id)
        {
            var payment = await _mediator.Send(new GetPaymentByIdQuery(id));
            if (payment == null) return NotFound();
            return View(payment);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request)
        {
            try
            {
                await _mediator.Send(new CreatePaymentCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Payment created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdatePaymentRequest request)
        {
            try
            {
                await _mediator.Send(new UpdatePaymentCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Payment updated successfully" });
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
                await _mediator.Send(new DeletePaymentCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Payment deleted successfully" });
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
                await _mediator.Send(new PostPaymentCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Payment posted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}