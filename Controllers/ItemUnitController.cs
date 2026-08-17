using AccuFlow.Models.Inventory;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class ItemUnitController : BaseController
    {
        private readonly IItemUnitService _itemUnitService;

        public ItemUnitController(IItemUnitService itemUnitService) : base(itemUnitService)
        {
            _itemUnitService = itemUnitService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Item Units";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableUnitRequest request) => Json(await _itemUnitService.Datatable(request));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _itemUnitService.GetById(id));

        [HttpGet]
        public async Task<IActionResult> GetActiveUnits() => Json(await _itemUnitService.GetActiveUnits());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUnitRequest request)
        {
            try
            {
                await _itemUnitService.Create(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Unit created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateUnitRequest request)
        {
            try
            {
                await _itemUnitService.Update(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Unit updated successfully" });
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
                await _itemUnitService.Delete(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Unit deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}