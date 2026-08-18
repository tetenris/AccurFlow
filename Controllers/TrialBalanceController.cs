using AccuFlow.Application.Features.TrialBalances.Queries;
using AccuFlow.Controllers;
using AccuFlow.Models.TrialBalance;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers;

[Authorize]
public class TrialBalanceController : BaseController
{
    private readonly ISender _mediator;

    public TrialBalanceController(ISender mediator, IBaseService baseService) : base(baseService)
    {
        _mediator = mediator;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Trial Balance";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetTrialBalance([FromQuery] GetTrialBalanceRequest request)
    {
        try
        {
            var result = await _mediator.Send(new GetTrialBalanceQuery(request));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel([FromQuery] GetTrialBalanceRequest request)
    {
        try
        {
            var fileBytes = await _mediator.Send(new ExportTrialBalanceQuery(request));
            var fileName = $"TrialBalance_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
