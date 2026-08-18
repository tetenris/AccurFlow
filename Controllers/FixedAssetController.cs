using AccuFlow.Application.Features.FixedAssets.Commands;
using AccuFlow.Application.Features.FixedAssets.Queries;
using AccuFlow.Models.FixedAsset;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class FixedAssetController : BaseController
    {
        private readonly ISender _mediator;

        public FixedAssetController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Fixed Assets";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableFixedAssetRequest request) => Json(await _mediator.Send(new GetFixedAssetDatatableQuery(request)));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _mediator.Send(new GetFixedAssetByIdQuery(id)));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFixedAssetRequest request)
        {
            try
            {
                await _mediator.Send(new CreateFixedAssetCommand(request, _currentUserService!.UserId));
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
                await _mediator.Send(new UpdateFixedAssetCommand(request, _currentUserService!.UserId));
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
                await _mediator.Send(new DeleteFixedAssetCommand(id, _currentUserService!.UserId));
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
                var deprec = await _mediator.Send(new DepreciateFixedAssetCommand(request.AssetId, request.PeriodDate, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Depreciation posted successfully", data = deprec });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}