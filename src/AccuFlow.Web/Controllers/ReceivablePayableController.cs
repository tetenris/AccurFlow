using AccuFlow.Application.Features.ReceivablePayables.Queries;
using AccuFlow.Models.ReceivablePayable;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class ReceivablePayableController : BaseController
    {
        private readonly ISender _mediator;

        public ReceivablePayableController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Receivable & Payable";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Generate([FromBody] ReceivablePayableRequest request) => Json(await _mediator.Send(new GetReceivablePayableQuery(request)));
    }
}