using AccuFlow.Application.Features.StockTransfers.Commands;
using AccuFlow.Application.Features.StockTransfers.Queries;
using AccuFlow.Models.StockTransfer;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class StockTransferController : BaseController
    {
        private readonly ISender _mediator;

        public StockTransferController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Stock Transfer";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableStockTransferRequest request) => Json(await _mediator.Send(new GetStockTransferDatatableQuery(request)));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _mediator.Send(new GetStockTransferByIdQuery(id)));

        [HttpGet]
        public async Task<IActionResult> GetWarehouses() => Json(await _mediator.Send(new GetStockTransferWarehousesQuery()));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockTransferRequest request)
        {
            try
            {
                await _mediator.Send(new CreateStockTransferCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Stock transfer created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateStockTransferRequest request)
        {
            try
            {
                await _mediator.Send(new UpdateStockTransferCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Stock transfer updated successfully" });
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
                await _mediator.Send(new PostStockTransferCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Stock transfer posted successfully" });
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
                await _mediator.Send(new DeleteStockTransferCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Stock transfer deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}