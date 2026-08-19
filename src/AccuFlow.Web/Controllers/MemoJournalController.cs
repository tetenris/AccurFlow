using AccuFlow.Application.Features.ChartOfAccounts.Queries;
using AccuFlow.Application.Features.JournalEntries.Commands;
using AccuFlow.Application.Features.JournalEntries.Queries;
using AccuFlow.Models.JournalEntry;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class MemoJournalController : BaseController
    {
        private readonly ISender _mediator;
        private readonly IBaseService _baseService;

        public MemoJournalController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
            _baseService = baseService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Jurnal Memo / Penyesuaian";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableJournalEntryRequest request) => Json(await _mediator.Send(new GetJournalDatatableQuery(request)));

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id) => Json(await _mediator.Send(new GetJournalByIdQuery(id)));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateJournalEntryRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.JournalType) || (request.JournalType != "Memo" && request.JournalType != "Adjustment"))
                {
                    return BadRequest(new { success = false, message = "Journal type must be Memo or Adjustment" });
                }
                var journalId = await _mediator.Send(new CreateJournalCommand(request, _currentUserService!.UserId));
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
                await _mediator.Send(new UpdateJournalCommand(request, _currentUserService!.UserId));
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
                await _mediator.Send(new PostJournalCommand(request, _currentUserService!.UserId));
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
                await _mediator.Send(new ReverseJournalCommand(request, _currentUserService!.UserId));
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
            var number = await _mediator.Send(new GenerateJournalNumberQuery(journalDate, journalType));
            return Json(new { number });
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountDropdown()
        {
            var accounts = await _mediator.Send(new GetCoaActiveAccountsQuery());
            var dropdown = accounts
                .Select(x => new { value = x.AccountId, text = $"{x.AccountCode} - {x.AccountName}" })
                .ToList();
            return Json(dropdown);
        }
    }
}