using AccuFlow.Models.Role;
using AccuFlow.Services;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    public class RoleController : BaseController
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService) : base(roleService)
        {
            _roleService = roleService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Role Management";
            return View();
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
                await _roleService.Create(request, _currentUserService.UserId);
                return Ok(new { success = true, message = "Role created successfully" });
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
    }
}
