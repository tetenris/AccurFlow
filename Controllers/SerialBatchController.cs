using AccuFlow.Models.BaseModel;
using AccuFlow.Models.StockBatch;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class SerialBatchController : BaseController
    {
        private readonly IStockBatchService _stockBatchService;

        public SerialBatchController(IStockBatchService stockBatchService, IBaseService baseService) : base(baseService)
        {
            _stockBatchService = stockBatchService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Serial Number / Batch";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] BaseDatatableRequest request, Guid? itemId = null)
        {
            var result = await _stockBatchService.Datatable(request, itemId);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> Items()
        {
            var items = await _stockBatchService.GetItems();
            var dropdown = items.Select(x => new { value = x.ItemId, text = $"{x.ItemCode} - {x.ItemName}" }).ToList();
            return Json(dropdown);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StockBatchCreateRequest request)
        {
            try
            {
                await _stockBatchService.Create(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = $"Batch {request.BatchNumber} registered successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Consume([FromBody] StockBatchConsumeRequest request)
        {
            try
            {
                await _stockBatchService.Consume(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Batch consumed successfully" });
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
                await _stockBatchService.Delete(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Batch deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}