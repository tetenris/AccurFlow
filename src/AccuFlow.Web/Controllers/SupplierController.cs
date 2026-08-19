using AccuFlow.Application.Features.Suppliers.Commands;
using AccuFlow.Application.Features.Suppliers.Queries;
using AccuFlow.Models.Supplier;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class SupplierController : BaseController
    {
        private readonly ISender _mediator;

        public SupplierController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Supplier Management";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableSupplierRequest request)
        {
            var result = await _mediator.Send(new GetSupplierDatatableQuery(request));
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetSupplierByIdQuery(id));
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveSuppliers()
        {
            var result = await _mediator.Send(new GetSupplierActiveQuery());
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request)
        {
            try
            {
                await _mediator.Send(new CreateSupplierCommand(request, _currentUserService.UserId));
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
                await _mediator.Send(new UpdateSupplierCommand(request, _currentUserService.UserId));
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
                await _mediator.Send(new DeleteSupplierCommand(id, userId));
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
                await _mediator.Send(new ToggleSupplierStatusCommand(id, _currentUserService.UserId));
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
            var isUnique = await _mediator.Send(new ValidateSupplierCodeQuery(code, excludeId));
            return Json(new { isUnique });
        }

        [HttpGet]
        public async Task<IActionResult> GenerateCode()
        {
            var code = await _mediator.Send(new GenerateSupplierCodeQuery());
            return Json(new { code });
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(string? supplierType, bool? isActive)
        {
            try
            {
                var fileBytes = await _mediator.Send(new ExportSupplierQuery(supplierType, isActive));
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