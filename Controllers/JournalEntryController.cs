using AccuFlow.Application.Features.ChartOfAccounts.Queries;
using AccuFlow.Application.Features.JournalEntries.Commands;
using AccuFlow.Application.Features.JournalEntries.Queries;
using AccuFlow.Models.JournalEntry;
using AccuFlow.Services;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class JournalEntryController : BaseController
    {
        private readonly ISender _mediator;
        private readonly AccuFlow.Infrastructure.Persistence.AppDbContext _dbContext;

        public JournalEntryController(
            ISender mediator,
            AccuFlow.Infrastructure.Persistence.AppDbContext dbContext,
            IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
            _dbContext = dbContext;
        }

        private async Task<Guid> GetUserIdAsync()
        {
            // Get user ID - if not authenticated, use user with Accounting role
            var userId = _currentUserService?.UserId ?? Guid.Empty;
            if (userId == Guid.Empty)
            {
                // Get user with Accounting role as default
                var accountingUser = await _dbContext.Set<UserEntity>()
                    .Include(x => x.Role)
                    .Where(x => !x.IsDeleted && x.Role.RoleName == "Accounting")
                    .FirstOrDefaultAsync();
                
                if (accountingUser == null)
                {
                    // Fallback to any active user
                    accountingUser = await _dbContext.Set<UserEntity>()
                        .Where(x => !x.IsDeleted)
                        .FirstOrDefaultAsync();
                }
                
                if (accountingUser != null)
                {
                    userId = accountingUser.UserId;
                }
            }
            return userId;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Journal Entry";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableJournalEntryRequest request)
        {
            var result = await _mediator.Send(new GetJournalDatatableQuery(request));
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetJournalByIdQuery(id));
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetByNumber(string number)
        {
            var result = await _mediator.Send(new GetJournalByNumberQuery(number));
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateJournalEntryRequest request)
        {
            try
            {
                var userId = await GetUserIdAsync();
                if (userId == Guid.Empty)
                {
                    return BadRequest(new { success = false, message = "No user found in system. Please create a user first." });
                }
                
                var journalId = await _mediator.Send(new CreateJournalCommand(request, userId));
                return Ok(new { success = true, message = "Journal entry created successfully", journalId });
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
                var userId = await GetUserIdAsync();
                await _mediator.Send(new UpdateJournalCommand(request, userId));
                return Ok(new { success = true, message = "Journal entry updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] Guid id)
        {
            try
            {
                var userId = await GetUserIdAsync();
                await _mediator.Send(new DeleteJournalCommand(id, userId));
                return Ok(new { success = true, message = "Journal entry deleted successfully" });
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
                var userId = await GetUserIdAsync();
                await _mediator.Send(new PostJournalCommand(request, userId));
                return Ok(new { success = true, message = "Journal entry posted successfully" });
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
                var userId = await GetUserIdAsync();
                await _mediator.Send(new ReverseJournalCommand(request, userId));
                return Ok(new { success = true, message = "Journal entry reversed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GenerateNumber(DateTime journalDate, string? journalType = null)
        {
            var number = await _mediator.Send(new GenerateJournalNumberQuery(journalDate, journalType));
            return Json(new { number });
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountDropdown()
        {
            var accounts = await _mediator.Send(new GetCoaDetailAccountsQuery());
            var dropdown = accounts
                .Select(x => new
                {
                    value = x.AccountId,
                    text = $"{x.AccountCode} - {x.AccountName}"
                })
                .ToList();
            return Json(dropdown);
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(DateTime? dateFrom, DateTime? dateTo, string? status, Guid? accountId)
        {
            try
            {
                var fileBytes = await _mediator.Send(new ExportJournalQuery(dateFrom, dateTo, status, accountId));
                var fileName = $"JournalEntries_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckAccounts()
        {
            var allAccounts = await _mediator.Send(new GetCoaDatatableQuery(new Models.ChartOfAccount.DataTableChartOfAccountRequest 
            { 
                Draw = 1, 
                Page = 1, 
                Size = 100 
            }));
            
            var detailAccounts = await _mediator.Send(new GetCoaDetailAccountsQuery());
            
            return Json(new 
            { 
                totalAccounts = allAccounts.RecordsTotal,
                detailAccounts = detailAccounts.Count,
                accounts = detailAccounts
            });
        }
    }
}