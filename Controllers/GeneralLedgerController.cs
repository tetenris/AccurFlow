using AccuFlow.Controllers;
using AccuFlow.Services;
using AccuFlow.Services.Interfaces;
using AccuFlow.Models.GeneralLedger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers;

[Authorize]
public class GeneralLedgerController : BaseController
{
    private readonly IGeneralLedgerService _generalLedgerService;
    private readonly IChartOfAccountService _chartOfAccountService;

    public GeneralLedgerController(
        IGeneralLedgerService generalLedgerService,
        IChartOfAccountService chartOfAccountService) : base(generalLedgerService)
    {
        _generalLedgerService = generalLedgerService;
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
            var result = await _generalLedgerService.GetAccountLedgerAsync(request);
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
            var result = await _generalLedgerService.GetLedgerSummaryAsync(request);
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
            var balance = await _generalLedgerService.GetAccountBalanceAsync(accountId, asOfDate);
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
            var fileBytes = await _generalLedgerService.ExportLedgerToExcelAsync(request);
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
            var fileBytes = await _generalLedgerService.ExportSummaryToExcelAsync(request);
            var fileName = $"LedgerSummary_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
