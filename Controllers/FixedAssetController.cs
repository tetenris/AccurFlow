using AccuFlow.Models.FixedAsset;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class FixedAssetController : BaseController
    {
        private readonly IFixedAssetService _fixedAssetService;

        public FixedAssetController(IFixedAssetService fixedAssetService) : base(fixedAssetService)
        {
            _fixedAssetService = fixedAssetService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Fixed Assets";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableFixedAssetRequest request) => Json(await _fixedAssetService.Datatable(request));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _fixedAssetService.GetById(id));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFixedAssetRequest request)
        {
            try
            {
                await _fixedAssetService.Create(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Fixed asset created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateFixedAssetRequest request)
        {
            try
            {
                await _fixedAssetService.Update(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Fixed asset updated successfully" });
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
                await _fixedAssetService.Delete(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Fixed asset deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Depreciate([FromBody] DepreciateFixedAssetRequest request)
        {
            try
            {
                var deprec = await _fixedAssetService.Depreciate(request.AssetId, request.PeriodDate, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Depreciation posted successfully", data = deprec });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}