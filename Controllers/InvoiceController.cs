using AccuFlow.Models.Invoice;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class InvoiceController : BaseController
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService) : base(invoiceService)
        {
            _invoiceService = invoiceService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Invoice Management";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableInvoiceRequest request) => Json(await _invoiceService.Datatable(request));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _invoiceService.GetById(id));

        [HttpGet]
        public async Task<IActionResult> GetOpenInvoices(string invoiceType)
        {
            var result = await _invoiceService.Datatable(new DataTableInvoiceRequest
            {
                InvoiceType = invoiceType,
                Page = 1,
                Size = 1000
            });

            return Json(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request)
        {
            try
            {
                await _invoiceService.Create(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Invoice created successfully" });
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
                await _invoiceService.Post(id, _currentUserService!.UserId);
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
                await _invoiceService.Cancel(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Invoice cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
