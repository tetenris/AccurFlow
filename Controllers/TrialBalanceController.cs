using AccuFlow.Controllers;
using AccuFlow.Models.TrialBalance;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers;

[Authorize]
public class TrialBalanceController : BaseController
{
    private readonly ITrialBalanceService _trialBalanceService;

    public TrialBalanceController(ITrialBalanceService trialBalanceService) : base(trialBalanceService)
    {
        _trialBalanceService = trialBalanceService;
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
            var result = await _trialBalanceService.GetTrialBalanceAsync(request);
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
            var fileBytes = await _trialBalanceService.ExportToExcelAsync(request);
            var fileName = $"TrialBalance_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
