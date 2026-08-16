using AccuFlow.Models.Tax;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class TaxController : BaseController
    {
        private readonly ITaxService _taxService;

        public TaxController(ITaxService taxService) : base(taxService)
        {
            _taxService = taxService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Taxes";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableTaxRequest request) => Json(await _taxService.Datatable(request));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _taxService.GetById(id));

        [HttpPost]
        public async Task<IActionResult> VatReport([FromBody] VatReportRequest request) => Json(await _taxService.VatReport(request));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaxRequest request)
        {
            try
            {
                await _taxService.Create(request, _currentUserService!.UserId);
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
                await _taxService.Update(request, _currentUserService!.UserId);
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
                await _taxService.Delete(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Tax deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}