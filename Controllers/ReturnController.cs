using AccuFlow.Models.Return;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class ReturnController : BaseController
    {
        private readonly IReturnService _returnService;

        public ReturnController(IReturnService returnService) : base(returnService)
        {
            _returnService = returnService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Sales & Purchase Returns";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableReturnRequest request) => Json(await _returnService.Datatable(request));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _returnService.GetById(id));

        [HttpGet]
        public async Task<IActionResult> GetWarehouses() => Json(await _returnService.GetWarehouses());

        [HttpGet]
        public async Task<IActionResult> GetInvoices(string returnType) => Json(await _returnService.GetInvoices(returnType));

        [HttpGet]
        public async Task<IActionResult> GetInvoiceLines(Guid invoiceId) => Json(await _returnService.GetInvoiceLines(invoiceId));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReturnRequest request)
        {
            try
            {
                await _returnService.Create(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Return created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateReturnRequest request)
        {
            try
            {
                await _returnService.Update(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Return updated successfully" });
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
                await _returnService.Post(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Return posted successfully" });
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
                await _returnService.Delete(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Return deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}