using AccuFlow.Models.Inventory;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class ItemGroupController : BaseController
    {
        private readonly IItemGroupService _itemGroupService;

        public ItemGroupController(IItemGroupService itemGroupService) : base(itemGroupService)
        {
            _itemGroupService = itemGroupService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Item Groups";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableItemGroupRequest request) => Json(await _itemGroupService.Datatable(request));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _itemGroupService.GetById(id));

        [HttpGet]
        public async Task<IActionResult> GetActiveGroups() => Json(await _itemGroupService.GetActiveGroups());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateItemGroupRequest request)
        {
            try
            {
                await _itemGroupService.Create(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Item group created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateItemGroupRequest request)
        {
            try
            {
                await _itemGroupService.Update(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Item group updated successfully" });
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
                await _itemGroupService.Delete(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Item group deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}