using AccuFlow.Models.GoodsReceipt;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class GoodsReceiptController : BaseController
    {
        private readonly IGoodsReceiptService _goodsReceiptService;

        public GoodsReceiptController(IGoodsReceiptService goodsReceiptService) : base(goodsReceiptService)
        {
            _goodsReceiptService = goodsReceiptService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Goods Received (GRN)";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableGoodsReceiptRequest request) => Json(await _goodsReceiptService.Datatable(request));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _goodsReceiptService.GetById(id));

        [HttpGet]
        public async Task<IActionResult> GetWarehouses() => Json(await _goodsReceiptService.GetWarehouses());

        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrders() => Json(await _goodsReceiptService.GetPurchaseOrders());

        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrderLines(Guid purchaseOrderId) => Json(await _goodsReceiptService.GetPurchaseOrderLines(purchaseOrderId));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGoodsReceiptRequest request)
        {
            try
            {
                await _goodsReceiptService.Create(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Goods receipt created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateGoodsReceiptRequest request)
        {
            try
            {
                await _goodsReceiptService.Update(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Goods receipt updated successfully" });
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
                await _goodsReceiptService.Post(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Goods receipt posted successfully" });
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
                await _goodsReceiptService.Delete(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Goods receipt deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}