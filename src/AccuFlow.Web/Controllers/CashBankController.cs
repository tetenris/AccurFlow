using AccuFlow.Application.Features.BankReconciliations.Commands;
using AccuFlow.Application.Features.BankReconciliations.Queries;
using AccuFlow.Application.Features.CashBankAccounts.Queries;
using AccuFlow.Application.Features.CashBankTransfers.Commands;
using AccuFlow.Application.Features.CashBankTransfers.Queries;
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
        private readonly ISender _mediator;

        public CashBankController(ISender mediator, IBaseService baseService) : base(baseService)
        {
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
        public async Task<IActionResult> GetBankStatement(Guid accountId, DateTime asOfDate) => Json(await _mediator.Send(new GetBankStatementQuery(accountId, asOfDate)));

        [HttpPost]
        public async Task<IActionResult> DatatableTransfers([FromBody] DataTableTransferRequest request) => Json(await _mediator.Send(new GetTransferDatatableQuery(request)));

        [HttpPost]
        public async Task<IActionResult> DatatableReconciliations([FromBody] DataTableReconciliationRequest request) => Json(await _mediator.Send(new GetReconciliationDatatableQuery(request)));

        [HttpGet]
        public async Task<IActionResult> GetTransferDetail(Guid id) => Json(await _mediator.Send(new GetTransferDetailQuery(id)));

        [HttpGet]
        public async Task<IActionResult> GetReconciliationDetail(Guid id) => Json(await _mediator.Send(new GetReconciliationDetailQuery(id)));

        [HttpPost]
        public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferRequest request)
        {
            try
            {
                await _mediator.Send(new CreateTransferCommand(request, _currentUserService!.UserId));
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
                await _mediator.Send(new UpdateTransferCommand(request, _currentUserService!.UserId));
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
                await _mediator.Send(new PostTransferCommand(id, _currentUserService!.UserId));
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
                await _mediator.Send(new DeleteTransferCommand(id, _currentUserService!.UserId));
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
                await _mediator.Send(new CreateReconciliationCommand(request, _currentUserService!.UserId));
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
                await _mediator.Send(new UpdateReconciliationCommand(request, _currentUserService!.UserId));
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
                await _mediator.Send(new PostReconciliationCommand(id, _currentUserService!.UserId));
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
                await _mediator.Send(new DeleteReconciliationCommand(id, _currentUserService!.UserId));
                return Ok(new { success = true, message = "Reconciliation deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
