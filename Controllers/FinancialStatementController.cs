using AccuFlow.Controllers;
using KomatsuERP.Models.FinancialStatement;
using KomatsuERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomatsuERP.Controllers;

[Authorize]
public class FinancialStatementController : BaseController
{
    private readonly IFinancialStatementService _financialStatementService;

    public FinancialStatementController(IFinancialStatementService financialStatementService) : base(financialStatementService)
    {
        _financialStatementService = financialStatementService;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Financial Statements";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetIncomeStatement([FromQuery] GetIncomeStatementRequest request)
    {
        try
        {
            var result = await _financialStatementService.GetIncomeStatementAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] GetBalanceSheetRequest request)
    {
        try
        {
            var result = await _financialStatementService.GetBalanceSheetAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCashFlow([FromQuery] GetCashFlowStatementRequest request)
    {
        try
        {
            var result = await _financialStatementService.GetCashFlowStatementAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportIncomeStatement([FromQuery] GetIncomeStatementRequest request)
    {
        try
        {
            var fileBytes = await _financialStatementService.ExportIncomeStatementAsync(request);
            var fileName = $"IncomeStatement_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportBalanceSheet([FromQuery] GetBalanceSheetRequest request)
    {
        try
        {
            var fileBytes = await _financialStatementService.ExportBalanceSheetAsync(request);
            var fileName = $"BalanceSheet_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportCashFlow([FromQuery] GetCashFlowStatementRequest request)
    {
        try
        {
            var fileBytes = await _financialStatementService.ExportCashFlowAsync(request);
            var fileName = $"CashFlow_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
