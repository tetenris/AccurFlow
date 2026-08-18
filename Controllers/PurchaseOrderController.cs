using AccuFlow.Application.Features.PurchaseOrders.Commands;
using AccuFlow.Application.Features.PurchaseOrders.Queries;
using AccuFlow.Models.PurchaseOrder;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class PurchaseOrderController : BaseController
    {
        private readonly ISender _mediator;

        public PurchaseOrderController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Purchase Orders";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTablePurchaseOrderRequest request) => Json(await _mediator.Send(new GetPurchaseOrderDatatableQuery(request)));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _mediator.Send(new GetPurchaseOrderByIdQuery(id)));

        [HttpGet]
        public async Task<IActionResult> Print(Guid id)
        {
            var purchaseOrder = await _mediator.Send(new GetPurchaseOrderByIdQuery(id));
            if (purchaseOrder == null) return NotFound();
            return View(purchaseOrder);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequest request)
        {
            try
            {
                await _mediator.Send(new CreatePurchaseOrderCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Purchase order created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdatePurchaseOrderRequest request)
        {
            try
            {
                await _mediator.Send(new UpdatePurchaseOrderCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Purchase order updated successfully" });
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
                await _mediator.Send(new DeletePurchaseOrderCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Purchase order deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Approve([FromBody] Guid id)
        {
            try
            {
                await _mediator.Send(new ApprovePurchaseOrderCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Purchase order approved successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConvertToInvoice([FromBody] Guid id)
        {
            try
            {
                await _mediator.Send(new ConvertPurchaseOrderToInvoiceCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Purchase order converted to invoice successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}