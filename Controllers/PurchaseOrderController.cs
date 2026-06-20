using AccuFlow.Models.PurchaseOrder;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class PurchaseOrderController : BaseController
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrderController(IPurchaseOrderService purchaseOrderService) : base(purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Purchase Orders";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTablePurchaseOrderRequest request) => Json(await _purchaseOrderService.Datatable(request));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequest request)
        {
            try
            {
                await _purchaseOrderService.Create(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Purchase order created successfully" });
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
                await _purchaseOrderService.Approve(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Purchase order approved successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConvertToInvoice([FromBody] Guid id)
        {
            try
            {
                await _purchaseOrderService.ConvertToInvoice(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Purchase order converted to invoice successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
