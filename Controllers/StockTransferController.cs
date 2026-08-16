using AccuFlow.Models.StockTransfer;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class StockTransferController : BaseController
    {
        private readonly IStockTransferService _stockTransferService;

        public StockTransferController(IStockTransferService stockTransferService) : base(stockTransferService)
        {
            _stockTransferService = stockTransferService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Stock Transfer";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableStockTransferRequest request) => Json(await _stockTransferService.Datatable(request));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _stockTransferService.GetById(id));

        [HttpGet]
        public async Task<IActionResult> GetWarehouses() => Json(await _stockTransferService.GetWarehouses());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockTransferRequest request)
        {
            try
            {
                await _stockTransferService.Create(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Stock transfer created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateStockTransferRequest request)
        {
            try
            {
                await _stockTransferService.Update(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Stock transfer updated successfully" });
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
                await _stockTransferService.Post(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Stock transfer posted successfully" });
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
                await _stockTransferService.Delete(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Stock transfer deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}