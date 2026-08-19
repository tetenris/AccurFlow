using AccuFlow.Application.Features.FinancialStatements.Queries;
using AccuFlow.Controllers;
using AccuFlow.Models.FinancialStatement;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers;

[Authorize]
public class FinancialStatementController : BaseController
{
    private readonly ISender _mediator;

    public FinancialStatementController(ISender mediator, IBaseService baseService) : base(baseService)
    {
        _mediator = mediator;
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
            var result = await _mediator.Send(new GetIncomeStatementQuery(request));
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
            var result = await _mediator.Send(new GetBalanceSheetQuery(request));
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
            var result = await _mediator.Send(new GetCashFlowQuery(request));
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
            var fileBytes = await _mediator.Send(new ExportIncomeStatementQuery(request));
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
            var fileBytes = await _mediator.Send(new ExportBalanceSheetQuery(request));
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
            var fileBytes = await _mediator.Send(new ExportCashFlowQuery(request));
            var fileName = $"CashFlow_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}