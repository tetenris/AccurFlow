using AccuFlow.Application.Features.ChartOfAccounts.Commands;
using AccuFlow.Application.Features.ChartOfAccounts.Queries;
using AccuFlow.Models.ChartOfAccount;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class ChartOfAccountController : BaseController
    {
        private readonly ISender _mediator;

        public ChartOfAccountController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Chart of Accounts";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableChartOfAccountRequest request)
        {
            var result = await _mediator.Send(new GetCoaDatatableQuery(request));
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetCoaByIdQuery(id));
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetHierarchy()
        {
            var result = await _mediator.Send(new GetCoaHierarchyQuery());
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveAccounts()
        {
            var result = await _mediator.Send(new GetCoaActiveAccountsQuery());
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetParentAccounts(string accountType)
        {
            var result = await _mediator.Send(new GetCoaParentAccountsQuery(accountType));
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateChartOfAccountRequest request)
        {
            try
            {
                await _mediator.Send(new CreateCoaCommand(request, _currentUserService.UserId));
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
                await _mediator.Send(new UpdateCoaCommand(request, _currentUserService.UserId));
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
                await _mediator.Send(new DeleteCoaCommand(id, _currentUserService.UserId));
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
                await _mediator.Send(new ToggleCoaStatusCommand(id, _currentUserService.UserId));
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
            var isUnique = await _mediator.Send(new ValidateCoaCodeQuery(code, excludeId));
            return Json(new { isUnique });
        }

        [HttpGet]
        public async Task<IActionResult> GenerateCode(Guid? parentId, string accountType)
        {
            var code = await _mediator.Send(new GenerateCoaCodeQuery(parentId, accountType));
            return Json(new { code });
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(string? accountType, bool? isActive)
        {
            try
            {
                var fileBytes = await _mediator.Send(new ExportCoaQuery(accountType, isActive));
                var fileName = $"ChartOfAccounts_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadTemplate()
        {
            try
            {
                var fileBytes = await _mediator.Send(new DownloadCoaTemplateQuery());
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ChartOfAccounts_Template.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            try
            {
                var result = await _mediator.Send(new ImportCoaCommand(file, _currentUserService.UserId));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}