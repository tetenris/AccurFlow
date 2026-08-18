using AccuFlow.Application.Features.Employees.Commands;
using AccuFlow.Application.Features.Employees.Queries;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Payroll;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class PayrollController : BaseController
    {
        private readonly IPayrollService _payrollService;
        private readonly ISender _mediator;

        public PayrollController(IPayrollService payrollService, ISender mediator, IBaseService baseService) : base(baseService)
        {
            _payrollService = payrollService;
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Payroll / HRM";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EmployeeDatatable([FromBody] BaseDatatableRequest request) => Json(await _mediator.Send(new GetEmployeeDatatableQuery(request)));

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeRequest request)
        {
            try
            {
                await _mediator.Send(new CreateEmployeeCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Employee created successfully" });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] EmployeeRequest request)
        {
            try
            {
                await _mediator.Send(new UpdateEmployeeCommand(id, request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Employee updated successfully" });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteEmployee([FromBody] Guid id)
        {
            try
            {
                await _mediator.Send(new DeleteEmployeeCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Employee deleted successfully" });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public async Task<IActionResult> PayrollDatatable([FromBody] BaseDatatableRequest request) => Json(await _payrollService.PayrollDatatable(request));

        [HttpPost]
        public async Task<IActionResult> CreatePayroll([FromBody] CreatePayrollRequest request)
        {
            try
            {
                await _payrollService.CreatePayroll(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = $"Payroll {request.PeriodMonth}/{request.PeriodYear} draft created" });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [HttpGet]
        public async Task<IActionResult> GetPayrollById(Guid id)
        {
            var data = await _payrollService.GetPayrollById(id);
            return Json(data);
        }

        [HttpPost]
        public async Task<IActionResult> PostPayroll([FromBody] Guid id)
        {
            try
            {
                await _payrollService.PostPayroll(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Payroll posted successfully (journal created)" });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePayroll([FromBody] Guid id)
        {
            try
            {
                await _payrollService.DeletePayroll(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Payroll draft deleted" });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }
    }
}