using AccuFlow.Application.Features.Customers.Commands;
using AccuFlow.Application.Features.Customers.Queries;
using AccuFlow.Models.Customer;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class CustomerController : BaseController
    {
        private readonly ISender _mediator;

        public CustomerController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Customer Management";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableCustomerRequest request)
        {
            var result = await _mediator.Send(new GetCustomerDatatableQuery(request));
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetCustomerByIdQuery(id));
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveCustomers()
        {
            var result = await _mediator.Send(new GetCustomerActiveQuery());
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
        {
            try
            {
                await _mediator.Send(new CreateCustomerCommand(request, _currentUserService.UserId));
                return Ok(new { success = true, message = "Customer created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateCustomerRequest request)
        {
            try
            {
                await _mediator.Send(new UpdateCustomerCommand(request, _currentUserService.UserId));
                return Ok(new { success = true, message = "Customer updated successfully" });
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
                await _mediator.Send(new DeleteCustomerCommand(id, userId));
                return Ok(new { success = true, message = "Customer deleted successfully" });
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
                await _mediator.Send(new ToggleCustomerStatusCommand(id, _currentUserService.UserId));
                return Ok(new { success = true, message = "Customer status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ValidateCode(string code, Guid? excludeId)
        {
            var isUnique = await _mediator.Send(new ValidateCustomerCodeQuery(code, excludeId));
            return Json(new { isUnique });
        }

        [HttpGet]
        public async Task<IActionResult> GenerateCode()
        {
            var code = await _mediator.Send(new GenerateCustomerCodeQuery());
            return Json(new { code });
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(string? customerType, bool? isActive)
        {
            try
            {
                var fileBytes = await _mediator.Send(new ExportCustomerQuery(customerType, isActive));
                var fileName = $"Customers_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}