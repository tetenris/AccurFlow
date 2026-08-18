using AccuFlow.Infrastructure.Persistence;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.JournalEntry;
using AccuFlow.Models.YearEndClosing;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface IYearEndClosingService : IBaseService
    {
        Task<YearEndClosingPreview> Preview(YearEndClosingRequest request);
        Task<YearEndClosingHistoryItem> Close(YearEndClosingRequest request, Guid userId);
        Task<List<YearEndClosingHistoryItem>> GetHistory();
    }

    public class YearEndClosingService : BaseService, IYearEndClosingService
    {
        private static readonly Guid RetainedEarningsAccountId = Guid.Parse("30000000-0000-0000-0000-000000000003");

        private readonly IGeneralLedgerService _generalLedgerService;
        private readonly IJournalEntryService _journalEntryService;

        public YearEndClosingService(
            AppDbContext dbContext,
            IGeneralLedgerService generalLedgerService,
            IJournalEntryService journalEntryService) : base(dbContext)
        {
            _generalLedgerService = generalLedgerService;
            _journalEntryService = journalEntryService;
        }

        public async Task<YearEndClosingPreview> Preview(YearEndClosingRequest request)
        {
            var endOfYear = new DateTime(request.FiscalYear, 12, 31);
            var accounts = await _dbContext.ChartOfAccounts
                .Where(a => !a.IsDeleted && a.IsActive && !a.IsHeader
                    && (a.AccountType == "Revenue" || a.AccountType == "Other Income"
                        || a.AccountType == "Expense" || a.AccountType == "Other Expense"))
                .OrderBy(a => a.AccountCode)
                .ToListAsync();

            var revenueAccounts = new List<YearEndClosingLinePreview>();
            var expenseAccounts = new List<YearEndClosingLinePreview>();
            decimal totalRevenue = 0;
            decimal totalExpense = 0;

            foreach (var account in accounts)
            {
                var balance = await _generalLedgerService.GetAccountBalanceAsync(account.AccountId, endOfYear);
                if (balance == 0) continue;
                var item = new YearEndClosingLinePreview
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountCode,
                    AccountName = account.AccountName,
                    Balance = balance
                };
                if (account.AccountType is "Revenue" or "Other Income")
                {
                    revenueAccounts.Add(item);
                    totalRevenue += balance;
                }
                else
                {
                    expenseAccounts.Add(item);
                    totalExpense += balance;
                }
            }

            return new YearEndClosingPreview
            {
                FiscalYear = request.FiscalYear,
                ClosingDate = endOfYear,
                RevenueAccounts = revenueAccounts,
                ExpenseAccounts = expenseAccounts,
                TotalRevenue = totalRevenue,
                TotalExpense = totalExpense,
                NetIncome = totalRevenue - totalExpense
            };
        }

        public async Task<YearEndClosingHistoryItem> Close(YearEndClosingRequest request, Guid userId)
        {
            var alreadyClosed = await _dbContext.YearEndClosings.AnyAsync(x => x.FiscalYear == request.FiscalYear && !x.IsDeleted);
            if (alreadyClosed) throw new Exception($"Fiscal year {request.FiscalYear} is already closed");

            var preview = await Preview(request);
            var endOfYear = new DateTime(request.FiscalYear, 12, 31);
            var description = $"Year-end closing for {request.FiscalYear}";

            var lines = new List<JournalLineRequest>();
            foreach (var revenue in preview.RevenueAccounts)
            {
                lines.Add(new JournalLineRequest
                {
                    AccountId = revenue.AccountId,
                    Description = description,
                    DebitAmount = revenue.Balance,
                    CreditAmount = 0
                });
            }
            foreach (var expense in preview.ExpenseAccounts)
            {
                lines.Add(new JournalLineRequest
                {
                    AccountId = expense.AccountId,
                    Description = description,
                    DebitAmount = 0,
                    CreditAmount = expense.Balance
                });
            }
            lines.Add(new JournalLineRequest
            {
                AccountId = RetainedEarningsAccountId,
                Description = description,
                DebitAmount = preview.TotalExpense,
                CreditAmount = preview.TotalRevenue
            });

            var journalId = await _journalEntryService.CreateAsync(new CreateJournalEntryRequest
            {
                JournalDate = endOfYear,
                Description = description,
                JournalLines = lines
            }, userId);
            await _journalEntryService.PostAsync(new PostJournalRequest { JournalId = journalId, PostedDate = endOfYear }, userId);

            var closing = new YearEndClosingEntity
            {
                ClosingId = Guid.NewGuid(),
                FiscalYear = request.FiscalYear,
                ClosingDate = endOfYear,
                ClosingJournalId = journalId,
                CreatedBy = userId.ToString()
            };
            _dbContext.YearEndClosings.Add(closing);
            await _dbContext.SaveChangesAsync();

            var journalNumber = await _dbContext.JournalEntries.Where(x => x.JournalId == journalId).Select(x => x.JournalNumber).FirstOrDefaultAsync();
            return new YearEndClosingHistoryItem
            {
                ClosingId = closing.ClosingId,
                FiscalYear = closing.FiscalYear,
                ClosingDate = closing.ClosingDate,
                ClosingJournalId = journalId,
                JournalNumber = journalNumber
            };
        }

        public async Task<List<YearEndClosingHistoryItem>> GetHistory()
        {
            return await _dbContext.YearEndClosings
                .Include(x => x.ClosingJournal)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.FiscalYear)
                .Select(x => new YearEndClosingHistoryItem
                {
                    ClosingId = x.ClosingId,
                    FiscalYear = x.FiscalYear,
                    ClosingDate = x.ClosingDate,
                    ClosingJournalId = x.ClosingJournalId,
                    JournalNumber = x.ClosingJournal != null ? x.ClosingJournal.JournalNumber : null
                }).ToListAsync();
        }
    }
}

