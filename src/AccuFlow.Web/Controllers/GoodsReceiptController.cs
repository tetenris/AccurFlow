using AccuFlow.Application.Features.GoodsReceipts.Commands;
using AccuFlow.Application.Features.GoodsReceipts.Queries;
using AccuFlow.Models.GoodsReceipt;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class GoodsReceiptController : BaseController
    {
        private readonly ISender _mediator;

        public GoodsReceiptController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Goods Received (GRN)";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableGoodsReceiptRequest request) => Json(await _mediator.Send(new GetGoodsReceiptDatatableQuery(request)));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _mediator.Send(new GetGoodsReceiptByIdQuery(id)));

        [HttpGet]
        public async Task<IActionResult> GetWarehouses() => Json(await _mediator.Send(new GetGoodsReceiptWarehousesQuery()));

        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrders() => Json(await _mediator.Send(new GetGoodsReceiptPurchaseOrdersQuery()));

        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrderLines(Guid purchaseOrderId) => Json(await _mediator.Send(new GetGoodsReceiptPurchaseOrderLinesQuery(purchaseOrderId)));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGoodsReceiptRequest request)
        {
            try
            {
                await _mediator.Send(new CreateGoodsReceiptCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Goods receipt created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateGoodsReceiptRequest request)
        {
            try
            {
                await _mediator.Send(new UpdateGoodsReceiptCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Goods receipt updated successfully" });
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
                await _mediator.Send(new PostGoodsReceiptCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Goods receipt posted successfully" });
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
                await _mediator.Send(new DeleteGoodsReceiptCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Goods receipt deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}