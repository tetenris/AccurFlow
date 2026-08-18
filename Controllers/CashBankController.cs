using AccuFlow.Application.Features.CashBankAccounts.Queries;
using AccuFlow.Models.CashBank;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class CashBankController : BaseController
    {
        private const int UsageCash = 1;
        private const int UsageBank = 2;
        private readonly ICashBankService _cashBankService;
        private readonly ISender _mediator;

        public CashBankController(ICashBankService cashBankService, ISender mediator, IBaseService baseService) : base(baseService)
        {
            _cashBankService = cashBankService;
            _mediator = mediator;
        }

        public async Task<IActionResult> Accounts()
        {
            ViewData["Title"] = "Cash & Bank Accounts";
            return View(await _mediator.Send(new GetCashBankAccountsQuery(UsageCash)));
        }

        public IActionResult Transfers()
        {
            ViewData["Title"] = "Cash Bank Transfers";
            return View();
        }

        public IActionResult Reconciliations()
        {
            ViewData["Title"] = "Bank Reconciliation";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCashAccounts() => Json(await _mediator.Send(new GetCashBankAccountsQuery(UsageCash)));

        [HttpGet]
        public async Task<IActionResult> GetBankAccounts() => Json(await _mediator.Send(new GetCashBankAccountsQuery(UsageBank)));

        [HttpGet]
        public async Task<IActionResult> GetBankStatement(Guid accountId, DateTime asOfDate) => Json(await _cashBankService.GetBankStatementAsync(accountId, asOfDate));

        [HttpPost]
        public async Task<IActionResult> DatatableTransfers([FromBody] DataTableTransferRequest request) => Json(await _cashBankService.GetTransfersAsync(request));

        [HttpPost]
        public async Task<IActionResult> DatatableReconciliations([FromBody] DataTableReconciliationRequest request) => Json(await _cashBankService.GetReconciliationsAsync(request));

        [HttpGet]
        public async Task<IActionResult> GetTransferDetail(Guid id) => Json(await _cashBankService.GetTransferDetailAsync(id));

        [HttpGet]
        public async Task<IActionResult> GetReconciliationDetail(Guid id) => Json(await _cashBankService.GetReconciliationDetailAsync(id));

        [HttpPost]
        public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferRequest request)
        {
            try
            {
                await _cashBankService.CreateTransferAsync(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Transfer created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTransfer([FromBody] UpdateTransferRequest request)
        {
            try
            {
                await _cashBankService.UpdateTransferAsync(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Transfer updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostTransfer([FromBody] Guid id)
        {
            try
            {
                await _cashBankService.PostTransferAsync(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Transfer posted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTransfer([FromBody] Guid id)
        {
            try
            {
                await _cashBankService.DeleteTransferAsync(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Transfer deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateReconciliation([FromBody] CreateReconciliationRequest request)
        {
            try
            {
                await _cashBankService.CreateReconciliationAsync(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Reconciliation created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReconciliation([FromBody] UpdateReconciliationRequest request)
        {
            try
            {
                await _cashBankService.UpdateReconciliationAsync(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Reconciliation updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostReconciliation([FromBody] Guid id)
        {
            try
            {
                await _cashBankService.PostReconciliationAsync(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Reconciliation posted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteReconciliation([FromBody] Guid id)
        {
            try
            {
                await _cashBankService.DeleteReconciliationAsync(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Reconciliation deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}