using AccuFlow.Models.Role;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class RoleController : BaseController
    {
        private readonly IRoleService _roleService;
        private readonly IRoleMenuService _roleMenuService;

        public RoleController(IRoleService roleService, IRoleMenuService roleMenuService) : base(roleService)
        {
            _roleService = roleService;
            _roleMenuService = roleMenuService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Role Management";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var role = await _roleService.GetById(id);
            if (role == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Edit Role - {role.RoleName}";
            ViewData["Back"] = "/Role/Index";
            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableRoleRequest request)
        {
            var result = await _roleService.Datatable(request);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _roleService.GetById(id);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
        {
            try
            {
                var roleId = await _roleService.Create(request, _currentUserService.UserId);
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
                await _roleService.Edit(request, _currentUserService.UserId);
                return Ok(new { success = true, message = "Role updated successfully" });
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
                await _roleService.Delete(id);
                return Ok(new { success = true, message = "Role deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetRoleDropdown()
        {
            var roles = _roleService.GetRoleDropdown();
            return Json(roles);
        }

        [HttpPost]
        public async Task<IActionResult> FixRoleTypeData()
        {
            try
            {
                await _roleService.FixRoleTypeData();
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
                
                var result = await _roleMenuService.GetRoleMenuPermissionsAsync(parsedRoleId);
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
                await _roleMenuService.SaveRoleMenuPermissionsAsync(request);
                return Ok(new { success = true, message = "Permissions saved successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
