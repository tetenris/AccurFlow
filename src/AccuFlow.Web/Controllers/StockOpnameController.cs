using AccuFlow.Application.Features.StockOpnames.Commands;
using AccuFlow.Application.Features.StockOpnames.Queries;
using AccuFlow.Models.StockOpname;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class StockOpnameController : BaseController
    {
        private readonly ISender _mediator;

        public StockOpnameController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Stock Opname";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableStockOpnameRequest request) => Json(await _mediator.Send(new GetStockOpnameDatatableQuery(request)));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _mediator.Send(new GetStockOpnameByIdQuery(id)));

        [HttpGet]
        public async Task<IActionResult> GetWarehouses() => Json(await _mediator.Send(new GetStockOpnameWarehousesQuery()));

        [HttpGet]
        public async Task<IActionResult> GetStockQuantities(Guid warehouseId) => Json(await _mediator.Send(new GetStockOpnameQuantitiesQuery(warehouseId)));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockOpnameRequest request)
        {
            try
            {
                await _mediator.Send(new CreateStockOpnameCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Stock opname created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateStockOpnameRequest request)
        {
            try
            {
                await _mediator.Send(new UpdateStockOpnameCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Stock opname updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Guid id)
        {
            try
            {
                await _mediator.Send(new PostStockOpnameCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Stock opname posted successfully" });
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
                await _mediator.Send(new DeleteStockOpnameCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Stock opname deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}