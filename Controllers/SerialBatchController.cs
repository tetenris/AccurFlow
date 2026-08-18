using AccuFlow.Application.Features.StockBatches.Commands;
using AccuFlow.Application.Features.StockBatches.Queries;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.StockBatch;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class SerialBatchController : BaseController
    {
        private readonly ISender _mediator;

        public SerialBatchController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Serial Number / Batch";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] BaseDatatableRequest request, Guid? itemId = null)
        {
            var result = await _mediator.Send(new GetStockBatchDatatableQuery(request, itemId));
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> Items()
        {
            var items = await _mediator.Send(new GetStockBatchItemsQuery());
            var dropdown = items.Select(x => new { value = x.ItemId, text = $"{x.ItemCode} - {x.ItemName}" }).ToList();
            return Json(dropdown);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StockBatchCreateRequest request)
        {
            try
            {
                await _mediator.Send(new CreateStockBatchCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = $"Batch {request.BatchNumber} registered successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Consume([FromBody] StockBatchConsumeRequest request)
        {
            try
            {
                await _mediator.Send(new ConsumeStockBatchCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Batch consumed successfully" });
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
                await _mediator.Send(new DeleteStockBatchCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Batch deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}