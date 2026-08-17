using AccuFlow.Models.SalesOrder;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class SalesOrderController : BaseController
    {
        private readonly ISalesOrderService _salesOrderService;

        public SalesOrderController(ISalesOrderService salesOrderService) : base(salesOrderService)
        {
            _salesOrderService = salesOrderService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Sales Order";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableOrderRequest request) => Json(await _salesOrderService.Datatable(request));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _salesOrderService.GetById(id));

        [HttpGet]
        public async Task<IActionResult> GetQuoteById(Guid id) => Json(await _salesOrderService.GetQuoteById(id));

        [HttpGet]
        public async Task<IActionResult> GetApprovedQuotes() => Json(await _salesOrderService.GetApprovedQuotes());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            try
            {
                await _salesOrderService.Create(request, _currentUserService!.UserId);
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
                await _salesOrderService.Update(request, _currentUserService!.UserId);
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
                await _salesOrderService.Approve(id, _currentUserService!.UserId);
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
                await _salesOrderService.Delete(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Sales order deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}