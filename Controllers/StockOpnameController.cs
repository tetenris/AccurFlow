using AccuFlow.Models.StockOpname;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class StockOpnameController : BaseController
    {
        private readonly IStockOpnameService _stockOpnameService;

        public StockOpnameController(IStockOpnameService stockOpnameService) : base(stockOpnameService)
        {
            _stockOpnameService = stockOpnameService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Stock Opname";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableStockOpnameRequest request) => Json(await _stockOpnameService.Datatable(request));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _stockOpnameService.GetById(id));

        [HttpGet]
        public async Task<IActionResult> GetWarehouses() => Json(await _stockOpnameService.GetWarehouses());

        [HttpGet]
        public async Task<IActionResult> GetStockQuantities(Guid warehouseId) => Json(await _stockOpnameService.GetStockQuantities(warehouseId));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockOpnameRequest request)
        {
            try
            {
                await _stockOpnameService.Create(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Stock opname created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateStockOpnameRequest request)
        {
            try
            {
                await _stockOpnameService.Update(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Stock opname updated successfully" });
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
                await _stockOpnameService.Post(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Stock opname posted successfully" });
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
                await _stockOpnameService.Delete(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Stock opname deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}