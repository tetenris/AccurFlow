using AccuFlow.Models.Inventory;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class InventoryController : BaseController
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService) : base(inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Inventory Items";
            return View();
        }

        public IActionResult StockCard()
        {
            ViewData["Title"] = "Stock Card";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DatatableItems([FromBody] DataTableItemRequest request) => Json(await _inventoryService.DatatableItems(request));

        [HttpGet]
        public async Task<IActionResult> GetActiveItems() => Json(await _inventoryService.GetActiveItems());

        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] CreateItemRequest request)
        {
            try
            {
                await _inventoryService.CreateItem(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Item created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> StockCardDatatable([FromBody] DataTableStockMovementRequest request) => Json(await _inventoryService.StockCard(request));
    }
}
