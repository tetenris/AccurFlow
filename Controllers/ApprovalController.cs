using AccuFlow.Application.Features.Approvals.Commands;
using AccuFlow.Application.Features.Approvals.Queries;
using AccuFlow.Models.Approval;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class ApprovalController : BaseController
    {
        private readonly ISender _mediator;

        public ApprovalController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Approval Workflow";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableApprovalRequest request) => Json(await _mediator.Send(new GetApprovalDatatableQuery(request)));

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitApprovalRequest request)
        {
            await _mediator.Send(new SubmitApprovalCommand(request, _currentUserService!.UserId));
            return Ok(new { success = true, message = "Approval submitted successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Approve([FromBody] ApprovalActionRequest request)
        {
            await _mediator.Send(new ApproveApprovalCommand(request, _currentUserService!.UserId));
            return Ok(new { success = true, message = "Document approved successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Reject([FromBody] ApprovalActionRequest request)
        {
            await _mediator.Send(new RejectApprovalCommand(request, _currentUserService!.UserId));
            return Ok(new { success = true, message = "Document rejected successfully" });
        }
    }
}
