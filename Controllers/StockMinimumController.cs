using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class StockMinimumController : BaseController
    {
        private readonly IInventoryService _inventoryService;

        public StockMinimumController(IInventoryService inventoryService) : base(inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Stock Minimum";
            return View();
        }
    }
}