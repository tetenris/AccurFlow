using AccuFlow.Models.Delivery;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class DeliveryOrderController : BaseController
    {
        private readonly IDeliveryOrderService _deliveryOrderService;

        public DeliveryOrderController(IDeliveryOrderService deliveryOrderService) : base(deliveryOrderService)
        {
            _deliveryOrderService = deliveryOrderService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Delivery Order";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableDeliveryRequest request) => Json(await _deliveryOrderService.Datatable(request));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _deliveryOrderService.GetById(id));

        [HttpGet]
        public async Task<IActionResult> GetOrders() => Json(await _deliveryOrderService.GetOrders());

        [HttpGet]
        public async Task<IActionResult> GetOrderLines(Guid salesOrderId) => Json(await _deliveryOrderService.GetOrderLines(salesOrderId));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDeliveryRequest request)
        {
            try
            {
                await _deliveryOrderService.Create(request, _currentUserService!.UserId);
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
                await _deliveryOrderService.Update(request, _currentUserService!.UserId);
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
                await _deliveryOrderService.Post(id, _currentUserService!.UserId);
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
                await _deliveryOrderService.ConvertToInvoice(id, _currentUserService!.UserId);
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
                await _deliveryOrderService.Delete(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Delivery order deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}