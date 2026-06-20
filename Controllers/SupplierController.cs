using AccuFlow.Models.Supplier;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class SupplierController : BaseController
    {
        private readonly ISupplierService _supplierService;

        public SupplierController(ISupplierService supplierService) : base(supplierService)
        {
            _supplierService = supplierService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Supplier Management";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableSupplierRequest request)
        {
            var result = await _supplierService.Datatable(request);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _supplierService.GetById(id);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveSuppliers()
        {
            var result = await _supplierService.GetActiveSuppliersAsync();
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request)
        {
            try
            {
                await _supplierService.Create(request, _currentUserService.UserId);
                return Ok(new { success = true, message = "Supplier created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateSupplierRequest request)
        {
            try
            {
                await _supplierService.Edit(request, _currentUserService.UserId);
                return Ok(new { success = true, message = "Supplier updated successfully" });
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
                var userId = Guid.Parse(User.FindFirst("UserId")?.Value ?? Guid.Empty.ToString());
                await _supplierService.Delete(id, userId);
                return Ok(new { success = true, message = "Supplier deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus([FromBody] Guid id)
        {
            try
            {
                await _supplierService.ToggleStatus(id, _currentUserService.UserId);
                return Ok(new { success = true, message = "Supplier status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ValidateCode(string code, Guid? excludeId)
        {
            var isUnique = await _supplierService.IsCodeUniqueAsync(code, excludeId);
            return Json(new { isUnique });
        }

        [HttpGet]
        public async Task<IActionResult> GenerateCode()
        {
            var code = await _supplierService.GenerateSupplierCodeAsync();
            return Json(new { code });
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(string? supplierType, bool? isActive)
        {
            try
            {
                var fileBytes = await _supplierService.ExportToExcelAsync(supplierType, isActive);
                var fileName = $"Suppliers_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
