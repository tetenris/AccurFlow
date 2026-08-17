using AccuFlow.Models.Quotation;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class SalesQuotationController : BaseController
    {
        private readonly IQuotationService _quotationService;

        public SalesQuotationController(IQuotationService quotationService) : base(quotationService)
        {
            _quotationService = quotationService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Sales Quotation";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableQuotationRequest request) => Json(await _quotationService.Datatable(request));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _quotationService.GetById(id));

        [HttpGet]
        public async Task<IActionResult> GetApprovedQuotes() => Json(await _quotationService.GetApprovedQuotes());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQuotationRequest request)
        {
            try
            {
                await _quotationService.Create(request, _currentUserService!.UserId);
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
                await _quotationService.Update(request, _currentUserService!.UserId);
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
                await _quotationService.Approve(id, _currentUserService!.UserId);
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
                await _quotationService.Delete(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Quotation deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}