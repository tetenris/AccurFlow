using AccuFlow.Application.Features.GeneralLedgers.Queries;
using AccuFlow.Services;
using AccuFlow.Models.GeneralLedger;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers;

[Authorize]
public class GeneralLedgerController : BaseController
{
    private readonly ISender _mediator;
    private readonly IChartOfAccountService _chartOfAccountService;

    public GeneralLedgerController(
        ISender mediator,
        IChartOfAccountService chartOfAccountService,
        IBaseService baseService) : base(baseService)
    {
        _mediator = mediator;
        _chartOfAccountService = chartOfAccountService;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "General Ledger";
        return View();
    }

    public IActionResult AccountLedger(Guid accountId)
    {
        ViewData["Title"] = "Account Ledger";
        ViewData["AccountId"] = accountId;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetLedger([FromQuery] GetLedgerRequest request)
    {
        try
        {
            var result = await _mediator.Send(new GetLedgerQuery(request));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetSummary([FromQuery] GetLedgerSummaryRequest request)
    {
        try
        {
            var result = await _mediator.Send(new GetLedgerSummaryQuery(request));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetBalance(Guid accountId, DateTime? asOfDate = null)
    {
        try
        {
            var balance = await _mediator.Send(new GetAccountBalanceQuery(accountId, asOfDate));
            return Ok(new { balance });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportLedger([FromQuery] GetLedgerRequest request)
    {
        try
        {
            var fileBytes = await _mediator.Send(new ExportLedgerQuery(request));
            var account = await _chartOfAccountService.GetByIdAsync(request.AccountId);
            var fileName = $"Ledger_{account?.AccountCode}_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportSummary([FromQuery] GetLedgerSummaryRequest request)
    {
        try
        {
            var fileBytes = await _mediator.Send(new ExportLedgerSummaryQuery(request));
            var fileName = $"LedgerSummary_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}