using AccuFlow.Models.ChartOfAccount;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class ChartOfAccountController : BaseController
    {
        private readonly IChartOfAccountService _chartOfAccountService;

        public ChartOfAccountController(IChartOfAccountService chartOfAccountService, IBaseService baseService) : base(baseService)
        {
            _chartOfAccountService = chartOfAccountService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Chart of Accounts";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableChartOfAccountRequest request)
        {
            var result = await _chartOfAccountService.Datatable(request);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _chartOfAccountService.GetByIdAsync(id);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetHierarchy()
        {
            var result = await _chartOfAccountService.GetHierarchyAsync();
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetParentAccounts(string accountType)
        {
            var result = await _chartOfAccountService.GetParentAccountsAsync(accountType);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateChartOfAccountRequest request)
        {
            try
            {
                await _chartOfAccountService.CreateAsync(request, _currentUserService.UserId);
                return Ok(new { success = true, message = "Account created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateChartOfAccountRequest request)
        {
            try
            {
                await _chartOfAccountService.UpdateAsync(request, _currentUserService.UserId);
                return Ok(new { success = true, message = "Account updated successfully" });
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
                await _chartOfAccountService.DeleteAsync(id, _currentUserService.UserId);
                return Ok(new { success = true, message = "Account deleted successfully" });
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
                await _chartOfAccountService.ToggleStatusAsync(id, _currentUserService.UserId);
                return Ok(new { success = true, message = "Account status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ValidateCode(string code, Guid? excludeId)
        {
            var isUnique = await _chartOfAccountService.IsCodeUniqueAsync(code, excludeId);
            return Json(new { isUnique });
        }

        [HttpGet]
        public async Task<IActionResult> GenerateCode(Guid? parentId, string accountType)
        {
            var code = await _chartOfAccountService.GenerateAccountCodeAsync(parentId, accountType);
            return Json(new { code });
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(string? accountType, bool? isActive)
        {
            try
            {
                var fileBytes = await _chartOfAccountService.ExportToExcelAsync(accountType, isActive);
                var fileName = $"ChartOfAccounts_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
