using AccuFlow.Models.Payment;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService) : base(paymentService)
        {
            _paymentService = paymentService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Payment & Receipt";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTablePaymentRequest request) => Json(await _paymentService.Datatable(request));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _paymentService.GetById(id));

        [HttpGet]
        public async Task<IActionResult> Print(Guid id)
        {
            var payment = await _paymentService.GetById(id);
            if (payment == null) return NotFound();
            return View(payment);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request)
        {
            try
            {
                await _paymentService.Create(request, _currentUserService!.UserId);
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
                await _paymentService.Edit(request, _currentUserService!.UserId);
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
                await _paymentService.Delete(id, _currentUserService!.UserId);
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
                await _paymentService.Post(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Payment posted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
