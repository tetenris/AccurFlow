using AccuFlow.Application.Features.SalesOrders.Commands;
using AccuFlow.Application.Features.SalesOrders.Queries;
using AccuFlow.Application.Features.SalesQuotations.Queries;
using AccuFlow.Models.SalesOrder;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class SalesOrderController : BaseController
    {
        private readonly ISender _mediator;

        public SalesOrderController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Sales Order";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableOrderRequest request) => Json(await _mediator.Send(new GetOrderDatatableQuery(request)));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _mediator.Send(new GetOrderByIdQuery(id)));

        [HttpGet]
        public async Task<IActionResult> GetQuoteById(Guid id) => Json(await _mediator.Send(new GetOrderQuoteByIdQuery(id)));

        [HttpGet]
        public async Task<IActionResult> GetApprovedQuotes() => Json(await _mediator.Send(new GetApprovedQuotesQuery()));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            try
            {
                await _mediator.Send(new CreateOrderCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Sales order created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateOrderRequest request)
        {
            try
            {
                await _mediator.Send(new UpdateOrderCommand(request, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Sales order updated successfully" });
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
                await _mediator.Send(new ApproveOrderCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Sales order approved successfully" });
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
                await _mediator.Send(new DeleteOrderCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Sales order deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}