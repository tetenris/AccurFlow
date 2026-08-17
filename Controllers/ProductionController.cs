using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Production;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class ProductionController : BaseController
    {
        private readonly IProductionService _productionService;

        public ProductionController(IProductionService productionService, IBaseService baseService) : base(baseService)
        {
            _productionService = productionService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Produksi / Manufacturing";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BomDatatable([FromBody] BaseDatatableRequest request) => Json(await _productionService.BomDatatable(request));

        [HttpGet]
        public async Task<IActionResult> GetBomById(Guid id) => Json(await _productionService.GetBomById(id));

        [HttpGet]
        public async Task<IActionResult> Items()
        {
            var items = await _productionService.GetItems();
            return Json(items.Select(x => new { value = x.ItemId, text = $"{x.ItemCode} - {x.ItemName}" }).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Warehouses()
        {
            var warehouses = await _productionService.GetWarehouses();
            return Json(warehouses.Select(x => new { value = x.WarehouseId, text = $"{x.WarehouseCode} - {x.WarehouseName}" }).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Boms() => Json(await _productionService.GetBoms());

        [HttpPost]
        public async Task<IActionResult> CreateBom([FromBody] BomRequest request)
        {
            try
            {
                await _productionService.CreateBom(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Bill of material created successfully" });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBom([FromBody] Guid id)
        {
            try
            {
                await _productionService.DeleteBom(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Bill of material deleted" });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public async Task<IActionResult> ProductionOrderDatatable([FromBody] BaseDatatableRequest request) => Json(await _productionService.ProductionOrderDatatable(request));

        [HttpPost]
        public async Task<IActionResult> CreateProductionOrder([FromBody] CreateProductionOrderRequest request)
        {
            try
            {
                await _productionService.CreateProductionOrder(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Production order draft created" });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductionOrderById(Guid id) => Json(await _productionService.GetProductionOrderById(id));

        [HttpPost]
        public async Task<IActionResult> PostProductionOrder([FromBody] Guid id)
        {
            try
            {
                await _productionService.PostProductionOrder(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Production order posted (stock + journal updated)" });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProductionOrder([FromBody] Guid id)
        {
            try
            {
                await _productionService.DeleteProductionOrder(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Production order draft deleted" });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }
    }
}