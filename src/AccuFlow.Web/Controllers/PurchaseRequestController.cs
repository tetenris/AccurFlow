using AccuFlow.Application.Features.PurchaseRequests.Commands;
using AccuFlow.Application.Features.PurchaseRequests.Queries;
using AccuFlow.Models.PurchaseRequest;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class PurchaseRequestController : BaseController
    {
        private readonly ISender _mediator;

        public PurchaseRequestController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Purchase Request";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTablePurchaseRequestRequest request) => Json(await _mediator.Send(new GetPurchaseRequestDatatableQuery(request)));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _mediator.Send(new GetPurchaseRequestByIdQuery(id)));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseRequestRequest request)
        {
            try
            {
                await _mediator.Send(new CreatePurchaseRequestCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Purchase request created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdatePurchaseRequestRequest request)
        {
            try
            {
                await _mediator.Send(new UpdatePurchaseRequestCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Purchase request updated successfully" });
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
                await _mediator.Send(new ApprovePurchaseRequestCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Purchase request approved successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConvertToPurchaseOrder([FromBody] ConvertPurchaseRequestRequest request)
        {
            try
            {
                var poId = await _mediator.Send(new ConvertPurchaseRequestCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Purchase order created from purchase request", purchaseOrderId = poId });
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
                await _mediator.Send(new DeletePurchaseRequestCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Purchase request deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}