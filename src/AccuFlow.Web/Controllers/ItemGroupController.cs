using AccuFlow.Application.Features.ItemGroups.Commands;
using AccuFlow.Application.Features.ItemGroups.Queries;
using AccuFlow.Models.Inventory;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class ItemGroupController : BaseController
    {
        private readonly ISender _mediator;

        public ItemGroupController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Item Groups";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableItemGroupRequest request) => Json(await _mediator.Send(new GetItemGroupDatatableQuery(request)));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _mediator.Send(new GetItemGroupByIdQuery(id)));

        [HttpGet]
        public async Task<IActionResult> GetActiveGroups() => Json(await _mediator.Send(new GetActiveItemGroupsQuery()));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateItemGroupRequest request)
        {
            try
            {
                await _mediator.Send(new CreateItemGroupCommand(request, _currentUserService!.UserId));
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
                await _mediator.Send(new UpdateItemGroupCommand(request, _currentUserService!.UserId));
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
                await _mediator.Send(new DeleteItemGroupCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Item group deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}