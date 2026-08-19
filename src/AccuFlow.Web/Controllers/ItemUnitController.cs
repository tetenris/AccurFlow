using AccuFlow.Application.Features.Units.Commands;
using AccuFlow.Application.Features.Units.Queries;
using AccuFlow.Models.Inventory;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class ItemUnitController : BaseController
    {
        private readonly ISender _mediator;

        public ItemUnitController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Item Units";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableUnitRequest request) => Json(await _mediator.Send(new GetUnitDatatableQuery(request)));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _mediator.Send(new GetUnitByIdQuery(id)));

        [HttpGet]
        public async Task<IActionResult> GetActiveUnits() => Json(await _mediator.Send(new GetActiveUnitsQuery()));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUnitRequest request)
        {
            try
            {
                await _mediator.Send(new CreateUnitCommand(request, _currentUserService!.UserId));
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
                await _mediator.Send(new UpdateUnitCommand(request, _currentUserService!.UserId));
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
                await _mediator.Send(new DeleteUnitCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Unit deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}