using AccuFlow.Application.Features.Users.Commands;
using AccuFlow.Application.Features.Users.Queries;
using AccuFlow.Models.User;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        private readonly ISender _mediator;

        public UserController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableUserRequest request)
        {
            var result = await _mediator.Send(new GetUsersDatatableQuery(request));
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _mediator.Send(new GetUserByIdQuery(id));
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }
            return Json(user);
        }

        [HttpGet]
        public async Task<IActionResult> GetRoleDropdown()
        {
            var roles = await _mediator.Send(new GetUserRoleDropdownQuery());
            var dropdown = roles.Select(r => new
            {
                value = r.RoleId.ToString(),
                text = r.RoleName
            });
            return Json(dropdown);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data" });
            }

            try
            {
                await _mediator.Send(new CreateUserCommand(model));
                return Ok(new { success = true, message = "User created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data" });
            }

            try
            {
                await _mediator.Send(new UpdateUserCommand(model));
                return Ok(new { success = true, message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Unlock([FromBody] Guid id)
        {
            try
            {
                await _mediator.Send(new UnlockUserCommand(id));
                return Ok(new { success = true, message = "User unlocked successfully. Password reset to default Qwerty@123 and user must change it on next login." });
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
                await _mediator.Send(new DeleteUserCommand(id));
                return Ok(new { success = true, message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
