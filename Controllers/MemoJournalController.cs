using AccuFlow.Models.JournalEntry;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class MemoJournalController : BaseController
    {
        private readonly IJournalEntryService _journalEntryService;
        private readonly IChartOfAccountService _chartOfAccountService;
        private readonly IBaseService _baseService;

        public MemoJournalController(
            IJournalEntryService journalEntryService,
            IChartOfAccountService chartOfAccountService,
            IBaseService baseService) : base(baseService)
        {
            _journalEntryService = journalEntryService;
            _chartOfAccountService = chartOfAccountService;
            _baseService = baseService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Jurnal Memo / Penyesuaian";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableJournalEntryRequest request)
        {
            var result = await _journalEntryService.Datatable(request);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _journalEntryService.GetByIdAsync(id));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateJournalEntryRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.JournalType) || (request.JournalType != "Memo" && request.JournalType != "Adjustment"))
                {
                    return BadRequest(new { success = false, message = "Journal type must be Memo or Adjustment" });
                }
                var journalId = await _journalEntryService.CreateAsync(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = $"{request.JournalType} journal created successfully", journalId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateJournalEntryRequest request)
        {
            try
            {
                await _journalEntryService.UpdateAsync(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Journal updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PostJournalRequest request)
        {
            try
            {
                await _journalEntryService.PostAsync(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Journal posted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Reverse([FromBody] ReverseJournalRequest request)
        {
            try
            {
                await _journalEntryService.ReverseAsync(request, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Journal reversed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GenerateNumber(DateTime journalDate, string journalType)
        {
            var number = await _journalEntryService.GenerateJournalNumberAsync(journalDate, journalType);
            return Json(new { number });
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountDropdown()
        {
            var accounts = await _chartOfAccountService.GetDetailAccountsAsync();
            var dropdown = accounts
                .Select(x => new { value = x.AccountId, text = $"{x.AccountCode} - {x.AccountName}" })
                .ToList();
            return Json(dropdown);
        }
    }
}