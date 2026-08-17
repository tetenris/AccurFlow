using AccuFlow.Models.ReceivablePayable;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class ReceivablePayableController : BaseController
    {
        private readonly IReceivablePayableService _receivablePayableService;

        public ReceivablePayableController(IReceivablePayableService receivablePayableService) : base(receivablePayableService)
        {
            _receivablePayableService = receivablePayableService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Receivable & Payable";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Generate([FromBody] ReceivablePayableRequest request) => Json(await _receivablePayableService.GetDetail(request));
    }
}