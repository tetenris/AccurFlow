using AccuFlow.Application.Features.Roles.Commands;
using AccuFlow.Application.Features.Roles.Queries;
using AccuFlow.Models.Role;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class RoleController : BaseController
    {
        private readonly ISender _mediator;

        public RoleController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Role Management";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var role = await _mediator.Send(new GetRoleByIdQuery(id));
            if (role == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Edit Role - {role.RoleName}";
            ViewData["Back"] = "/Role/Index";
            return View(role);
        }

        [HttpGet]
        public async Task<IActionResult> Permissions(Guid id)
        {
            var role = await _mediator.Send(new GetRoleByIdQuery(id));
            if (role == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Role Permissions - {role.RoleName}";
            ViewData["Back"] = "/Role/Index";
            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableRoleRequest request)
        {
            var result = await _mediator.Send(new GetRolesDatatableQuery(request));
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetRoleByIdQuery(id));
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
        {
            try
            {
                var roleId = await _mediator.Send(new CreateRoleCommand(request, _currentUserService.UserId));
                return Ok(new { success = true, message = "Role created successfully", roleId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] EditRoleRequest request)
        {
            try
            {
                await _mediator.Send(new EditRoleCommand(request, _currentUserService.UserId));
                return Ok(new { success = true, message = "Role updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRoleDropdown()
        {
            var roles = await _mediator.Send(new GetRoleDropdownQuery());
            return Json(roles);
        }

        [HttpPost]
        public async Task<IActionResult> FixRoleTypeData()
        {
            try
            {
                await _mediator.Send(new FixRoleTypeDataCommand());
                return Ok(new { success = true, message = "Role type data fixed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRoleMenuPermissions(string roleId = "")
        {
            try
            {
                Guid parsedRoleId = Guid.Empty;
                if (!string.IsNullOrEmpty(roleId))
                {
                    Guid.TryParse(roleId, out parsedRoleId);
                }

                var result = await _mediator.Send(new GetRoleMenuPermissionsQuery(parsedRoleId));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveRoleMenuPermissions([FromBody] AccuFlow.Models.RoleMenu.SaveRoleMenuRequest request)
        {
            try
            {
                await _mediator.Send(new SaveRoleMenuPermissionsCommand(request));
                return Ok(new { success = true, message = "Permissions saved successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
