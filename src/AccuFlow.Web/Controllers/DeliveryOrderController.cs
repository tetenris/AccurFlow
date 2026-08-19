using AccuFlow.Application.Features.DeliveryOrders.Commands;
using AccuFlow.Application.Features.DeliveryOrders.Queries;
using AccuFlow.Models.Delivery;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class DeliveryOrderController : BaseController
    {
        private readonly ISender _mediator;

        public DeliveryOrderController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Delivery Order";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableDeliveryRequest request) => Json(await _mediator.Send(new GetDeliveryDatatableQuery(request)));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _mediator.Send(new GetDeliveryByIdQuery(id)));

        [HttpGet]
        public async Task<IActionResult> GetOrders() => Json(await _mediator.Send(new GetDeliveryOrdersQuery()));

        [HttpGet]
        public async Task<IActionResult> GetOrderLines(Guid salesOrderId) => Json(await _mediator.Send(new GetDeliveryOrderLinesQuery(salesOrderId)));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDeliveryRequest request)
        {
            try
            {
                await _mediator.Send(new CreateDeliveryCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Delivery order created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateDeliveryRequest request)
        {
            try
            {
                await _mediator.Send(new UpdateDeliveryCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Delivery order updated successfully" });
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
                await _mediator.Send(new PostDeliveryCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Delivery order posted successfully" });
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
                await _mediator.Send(new ConvertDeliveryToInvoiceCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Delivery order converted to invoice successfully" });
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
                await _mediator.Send(new DeleteDeliveryCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Delivery order deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}