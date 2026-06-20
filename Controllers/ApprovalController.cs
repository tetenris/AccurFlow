using AccuFlow.Models.Approval;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class ApprovalController : BaseController
    {
        private readonly IApprovalService _approvalService;

        public ApprovalController(IApprovalService approvalService) : base(approvalService)
        {
            _approvalService = approvalService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Approval Workflow";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableApprovalRequest request) => Json(await _approvalService.Datatable(request));

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitApprovalRequest request)
        {
            await _approvalService.Submit(request, _currentUserService!.UserId);
            return Ok(new { success = true, message = "Approval submitted successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Approve([FromBody] ApprovalActionRequest request)
        {
            await _approvalService.Approve(request, _currentUserService!.UserId);
            return Ok(new { success = true, message = "Document approved successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Reject([FromBody] ApprovalActionRequest request)
        {
            await _approvalService.Reject(request, _currentUserService!.UserId);
            return Ok(new { success = true, message = "Document rejected successfully" });
        }
    }
}
