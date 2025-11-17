using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using AccuFlow.Infrastructures;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.JournalEntry;
using KomatsuERP.Models.JournalEntry;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace AccuFlow.Services
{
    public interface IJournalEntryService : IBaseService
    {
        // CRUD Operations
        Task<BaseDatatableResponse> Datatable(DataTableJournalEntryRequest request);
        Task<JournalEntryDetailViewModel?> GetByIdAsync(Guid id);
        Task<JournalEntryDetailViewModel?> GetByNumberAsync(string journalNumber);
        Task<Guid> CreateAsync(CreateJournalEntryRequest request, Guid userId);
        Task UpdateAsync(UpdateJournalEntryRequest request, Guid userId);
        Task DeleteAsync(Guid id, Guid userId);

        // Status Operations
        Task PostAsync(PostJournalRequest request, Guid userId);
        Task ReverseAsync(ReverseJournalRequest request, Guid userId);

        // Validation
        Task<bool> IsBalancedAsync(Guid journalId);
        Task<bool> CanEditAsync(Guid journalId);
        Task<bool> CanDeleteAsync(Guid journalId);
        Task<bool> CanPostAsync(Guid journalId);
        Task<bool> CanReverseAsync(Guid journalId);
        Task<List<string>> ValidateJournalAsync(CreateJournalEntryRequest request);

        // Utility
        Task<string> GenerateJournalNumberAsync(DateTime journalDate);
        Task<List<JournalEntryViewModel>> GetByAccountAsync(Guid accountId);
        Task<List<JournalEntryViewModel>> GetByDateRangeAsync(DateTime dateFrom, DateTime dateTo);

        // Export
        Task<byte[]> ExportToExcelAsync(DateTime? dateFrom, DateTime? dateTo, string? status, Guid? accountId);
    }

    public class JournalEntryService : BaseService, IJournalEntryService
    {
        private readonly ICurrentUserService _currentUserService;

        public JournalEntryService(AppDbContext dbContext, ICurrentUserService currentUserService) : base(dbContext)
        {
            _currentUserService = currentUserService;
        }

        public async Task<BaseDatatableResponse> Datatable(DataTableJournalEntryRequest request)
        {
            var query = _dbContext.Set<JournalEntryEntity>()
                .Where(x => !x.IsDeleted)
                .Include(x => x.CreatedByUser)
                .Include(x => x.PostedByUser)
                .AsQueryable();

            // Apply filters
            if (request.DateFrom.HasValue)
            {
                query = query.Where(x => x.JournalDate >= request.DateFrom.Value);
            }

            if (request.DateTo.HasValue)
            {
                query = query.Where(x => x.JournalDate <= request.DateTo.Value);
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(x => x.Status == request.Status);
            }

            if (request.AccountId.HasValue)
            {
                query = query.Where(x => x.JournalLines.Any(l => l.AccountId == request.AccountId.Value && !l.IsDeleted));
            }

            // Search
            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(x => x.JournalNumber.Contains(request.Search) || x.Description.Contains(request.Search));
            }

            // Total records
            var totalRecords = await query.CountAsync();

            // Sorting
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                query = query.OrderBy($"{request.OrderBy} {request.OrderType}");
            }
            else
            {
                query = query.OrderByDescending(x => x.JournalDate).ThenByDescending(x => x.JournalNumber);
            }

            // Pagination
            var data = await query
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .Select(x => new JournalEntryViewModel
                {
                    JournalId = x.JournalId,
                    JournalNumber = x.JournalNumber,
                    JournalDate = x.JournalDate,
                    Description = x.Description,
                    Status = x.Status,
                    TotalDebit = x.TotalDebit,
                    TotalCredit = x.TotalCredit,
                    IsBalanced = x.IsBalanced,
                    PostedDate = x.PostedDate,
                    PostedBy = x.PostedByUser != null ? x.PostedByUser.FullName : null,
                    CreatedBy = x.CreatedByUser != null ? x.CreatedByUser.FullName : "",
                    CreatedAt = x.CreatedAt,
                    CanEdit = x.CanEdit,
                    CanDelete = x.CanDelete,
                    CanPost = x.CanPost,
                    CanReverse = x.CanReverse
                })
                .ToListAsync();

            return new BaseDatatableResponse
            {
                Draw = request.Draw,
                RecordsTotal = totalRecords,
                RecordsFiltered = totalRecords,
                Data = data
            };
        }

        public async Task<JournalEntryDetailViewModel?> GetByIdAsync(Guid id)
        {
            var journal = await _dbContext.Set<JournalEntryEntity>()
                .Where(x => x.JournalId == id && !x.IsDeleted)
                .Include(x => x.JournalLines.Where(l => !l.IsDeleted))
                    .ThenInclude(l => l.Account)
                .Include(x => x.CreatedByUser)
                .Include(x => x.UpdatedByUser)
                .Include(x => x.PostedByUser)
                .Include(x => x.ReversalJournal)
                .Include(x => x.OriginalJournal)
                .FirstOrDefaultAsync();

            if (journal == null) return null;

            return new JournalEntryDetailViewModel
            {
                JournalId = journal.JournalId,
                JournalNumber = journal.JournalNumber,
                JournalDate = journal.JournalDate,
                Description = journal.Description,
                Status = journal.Status,
                TotalDebit = journal.TotalDebit,
                TotalCredit = journal.TotalCredit,
                IsBalanced = journal.IsBalanced,
                PostedDate = journal.PostedDate,
                PostedBy = journal.PostedByUser?.FullName,
                ReversalJournalNumber = journal.ReversalJournal?.JournalNumber,
                OriginalJournalNumber = journal.OriginalJournal?.JournalNumber,
                CreatedBy = journal.CreatedByUser?.FullName ?? "",
                CreatedAt = journal.CreatedAt,
                UpdatedBy = journal.UpdatedByUser?.FullName,
                UpdatedAt = journal.UpdatedAt,
                CanEdit = journal.CanEdit,
                CanDelete = journal.CanDelete,
                CanPost = journal.CanPost,
                CanReverse = journal.CanReverse,
                JournalLines = journal.JournalLines
                    .OrderBy(l => l.LineNumber)
                    .Select(l => new JournalLineViewModel
                    {
                        JournalLineId = l.JournalLineId,
                        LineNumber = l.LineNumber,
                        AccountId = l.AccountId,
                        AccountCode = l.Account.AccountCode,
                        AccountName = l.Account.AccountName,
                        Description = l.Description,
                        DebitAmount = l.DebitAmount,
                        CreditAmount = l.CreditAmount
                    })
                    .ToList()
            };
        }

        public async Task<JournalEntryDetailViewModel?> GetByNumberAsync(string journalNumber)
        {
            var journal = await _dbContext.Set<JournalEntryEntity>()
                .Where(x => x.JournalNumber == journalNumber && !x.IsDeleted)
                .FirstOrDefaultAsync();

            if (journal == null) return null;

            return await GetByIdAsync(journal.JournalId);
        }

        public async Task<Guid> CreateAsync(CreateJournalEntryRequest request, Guid userId)
        {
            // Validate
            var errors = await ValidateJournalAsync(request);
            if (errors.Any())
            {
                throw new Exception(string.Join(", ", errors));
            }

            // Generate journal number
            var journalNumber = await GenerateJournalNumberAsync(request.JournalDate);

            // Calculate totals
            var totalDebit = request.JournalLines.Sum(x => x.DebitAmount);
            var totalCredit = request.JournalLines.Sum(x => x.CreditAmount);

            // Create journal entry
            var journal = new JournalEntryEntity
            {
                JournalId = Guid.NewGuid(),
                JournalNumber = journalNumber,
                JournalDate = request.JournalDate,
                Description = request.Description,
                Status = "Draft",
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            // Create journal lines
            int lineNumber = 1;
            foreach (var line in request.JournalLines)
            {
                journal.JournalLines.Add(new JournalLineEntity
                {
                    JournalLineId = Guid.NewGuid(),
                    LineNumber = lineNumber++,
                    AccountId = line.AccountId,
                    Description = line.Description,
                    DebitAmount = line.DebitAmount,
                    CreditAmount = line.CreditAmount,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _dbContext.Set<JournalEntryEntity>().Add(journal);
            await _dbContext.SaveChangesAsync();

            return journal.JournalId;
        }

        public async Task UpdateAsync(UpdateJournalEntryRequest request, Guid userId)
        {
            var journal = await _dbContext.Set<JournalEntryEntity>()
                .Include(x => x.JournalLines)
                .FirstOrDefaultAsync(x => x.JournalId == request.JournalId && !x.IsDeleted);

            if (journal == null)
            {
                throw new Exception("Journal entry not found");
            }

            if (!journal.CanEdit)
            {
                throw new Exception("Cannot edit this journal entry");
            }

            // Validate
            var errors = await ValidateJournalAsync(new CreateJournalEntryRequest
            {
                JournalDate = request.JournalDate,
                Description = request.Description,
                JournalLines = request.JournalLines
            });

            if (errors.Any())
            {
                throw new Exception(string.Join(", ", errors));
            }

            // Update header
            journal.JournalDate = request.JournalDate;
            journal.Description = request.Description;
            journal.TotalDebit = request.JournalLines.Sum(x => x.DebitAmount);
            journal.TotalCredit = request.JournalLines.Sum(x => x.CreditAmount);
            journal.UpdatedBy = userId;
            journal.UpdatedAt = DateTime.UtcNow;

            // Delete existing lines
            foreach (var line in journal.JournalLines)
            {
                line.IsDeleted = true;
            }

            // Add new lines
            int lineNumber = 1;
            foreach (var line in request.JournalLines)
            {
                journal.JournalLines.Add(new JournalLineEntity
                {
                    JournalLineId = Guid.NewGuid(),
                    LineNumber = lineNumber++,
                    AccountId = line.AccountId,
                    Description = line.Description,
                    DebitAmount = line.DebitAmount,
                    CreditAmount = line.CreditAmount,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id, Guid userId)
        {
            var journal = await _dbContext.Set<JournalEntryEntity>()
                .Include(x => x.JournalLines)
                .FirstOrDefaultAsync(x => x.JournalId == id && !x.IsDeleted);

            if (journal == null)
            {
                throw new Exception("Journal entry not found");
            }

            if (!journal.CanDelete)
            {
                throw new Exception("Cannot delete this journal entry");
            }

            journal.IsDeleted = true;
            journal.DeletedBy = userId;
            journal.DeletedAt = DateTime.UtcNow;

            foreach (var line in journal.JournalLines)
            {
                line.IsDeleted = true;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task PostAsync(PostJournalRequest request, Guid userId)
        {
            var journal = await _dbContext.Set<JournalEntryEntity>()
                .Include(x => x.JournalLines)
                    .ThenInclude(l => l.Account)
                .FirstOrDefaultAsync(x => x.JournalId == request.JournalId && !x.IsDeleted);

            if (journal == null)
            {
                throw new Exception("Journal entry not found");
            }

            if (!journal.CanPost)
            {
                throw new Exception("Cannot post this journal entry");
            }

            // Validate all accounts still exist and active
            foreach (var line in journal.JournalLines.Where(l => !l.IsDeleted))
            {
                if (line.Account == null || !line.Account.IsActive)
                {
                    throw new Exception($"Account {line.Account?.AccountCode} is not active");
                }
            }

            journal.Status = "Posted";
            journal.PostedDate = request.PostedDate;
            journal.PostedBy = userId;
            journal.UpdatedBy = userId;
            journal.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task ReverseAsync(ReverseJournalRequest request, Guid userId)
        {
            var journal = await _dbContext.Set<JournalEntryEntity>()
                .Include(x => x.JournalLines.Where(l => !l.IsDeleted))
                    .ThenInclude(l => l.Account)
                .FirstOrDefaultAsync(x => x.JournalId == request.JournalId && !x.IsDeleted);

            if (journal == null)
            {
                throw new Exception("Journal entry not found");
            }

            if (!journal.CanReverse)
            {
                throw new Exception("Cannot reverse this journal entry");
            }

            // Generate reversal journal number
            var reversalNumber = await GenerateJournalNumberAsync(request.ReversalDate);

            // Create reversal journal
            var reversalJournal = new JournalEntryEntity
            {
                JournalId = Guid.NewGuid(),
                JournalNumber = reversalNumber,
                JournalDate = request.ReversalDate,
                Description = $"Reversal of {journal.JournalNumber} - {journal.Description}",
                Status = "Posted",
                TotalDebit = journal.TotalCredit, // Swap
                TotalCredit = journal.TotalDebit, // Swap
                PostedDate = request.ReversalDate,
                PostedBy = userId,
                OriginalJournalId = journal.JournalId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            // Create reversed lines (swap debit/credit)
            int lineNumber = 1;
            foreach (var line in journal.JournalLines.OrderBy(l => l.LineNumber))
            {
                reversalJournal.JournalLines.Add(new JournalLineEntity
                {
                    JournalLineId = Guid.NewGuid(),
                    LineNumber = lineNumber++,
                    AccountId = line.AccountId,
                    Description = line.Description,
                    DebitAmount = line.CreditAmount, // Swap
                    CreditAmount = line.DebitAmount, // Swap
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Update original journal
            journal.Status = "Reversed";
            journal.ReversalJournalId = reversalJournal.JournalId;
            journal.UpdatedBy = userId;
            journal.UpdatedAt = DateTime.UtcNow;

            _dbContext.Set<JournalEntryEntity>().Add(reversalJournal);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> IsBalancedAsync(Guid journalId)
        {
            var journal = await _dbContext.Set<JournalEntryEntity>()
                .FirstOrDefaultAsync(x => x.JournalId == journalId && !x.IsDeleted);

            return journal?.IsBalanced ?? false;
        }

        public async Task<bool> CanEditAsync(Guid journalId)
        {
            var journal = await _dbContext.Set<JournalEntryEntity>()
                .FirstOrDefaultAsync(x => x.JournalId == journalId && !x.IsDeleted);

            return journal?.CanEdit ?? false;
        }

        public async Task<bool> CanDeleteAsync(Guid journalId)
        {
            var journal = await _dbContext.Set<JournalEntryEntity>()
                .FirstOrDefaultAsync(x => x.JournalId == journalId && !x.IsDeleted);

            return journal?.CanDelete ?? false;
        }

        public async Task<bool> CanPostAsync(Guid journalId)
        {
            var journal = await _dbContext.Set<JournalEntryEntity>()
                .FirstOrDefaultAsync(x => x.JournalId == journalId && !x.IsDeleted);

            return journal?.CanPost ?? false;
        }

        public async Task<bool> CanReverseAsync(Guid journalId)
        {
            var journal = await _dbContext.Set<JournalEntryEntity>()
                .FirstOrDefaultAsync(x => x.JournalId == journalId && !x.IsDeleted);

            return journal?.CanReverse ?? false;
        }

        public async Task<List<string>> ValidateJournalAsync(CreateJournalEntryRequest request)
        {
            var errors = new List<string>();

            // Check journal date
            if (request.JournalDate > DateTime.Now)
            {
                errors.Add("Journal date cannot be in the future");
            }

            // Check minimum lines
            if (request.JournalLines.Count < 2)
            {
                errors.Add("At least 2 journal lines are required");
            }

            // Check balance
            var totalDebit = request.JournalLines.Sum(x => x.DebitAmount);
            var totalCredit = request.JournalLines.Sum(x => x.CreditAmount);

            if (totalDebit != totalCredit)
            {
                errors.Add($"Journal is not balanced. Debit: {totalDebit}, Credit: {totalCredit}");
            }

            // Check amounts
            foreach (var line in request.JournalLines)
            {
                if (line.DebitAmount < 0 || line.CreditAmount < 0)
                {
                    errors.Add("Amounts must be positive");
                }

                if (line.DebitAmount > 0 && line.CreditAmount > 0)
                {
                    errors.Add("A line cannot have both debit and credit amounts");
                }

                if (line.DebitAmount == 0 && line.CreditAmount == 0)
                {
                    errors.Add("A line must have either debit or credit amount");
                }
            }

            // Check accounts
            var accountIds = request.JournalLines.Select(x => x.AccountId).Distinct().ToList();
            var accounts = await _dbContext.Set<ChartOfAccountEntity>()
                .Where(x => accountIds.Contains(x.AccountId) && !x.IsDeleted)
                .ToListAsync();

            foreach (var accountId in accountIds)
            {
                var account = accounts.FirstOrDefault(x => x.AccountId == accountId);
                if (account == null)
                {
                    errors.Add($"Account not found");
                }
                else
                {
                    if (!account.IsActive)
                    {
                        errors.Add($"Account {account.AccountCode} is not active");
                    }

                    if (account.IsHeader)
                    {
                        errors.Add($"Account {account.AccountCode} is a header account and cannot be used in journal entries");
                    }
                }
            }

            return errors;
        }

        public async Task<string> GenerateJournalNumberAsync(DateTime journalDate)
        {
            var datePrefix = journalDate.ToString("yyyyMMdd");
            var prefix = $"JE-{datePrefix}-";

            var lastJournal = await _dbContext.Set<JournalEntryEntity>()
                .Where(x => x.JournalNumber.StartsWith(prefix))
                .OrderByDescending(x => x.JournalNumber)
                .FirstOrDefaultAsync();

            if (lastJournal != null)
            {
                var lastNumber = lastJournal.JournalNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumber, out int number))
                {
                    return $"{prefix}{(number + 1):D4}";
                }
            }

            return $"{prefix}0001";
        }

        public async Task<List<JournalEntryViewModel>> GetByAccountAsync(Guid accountId)
        {
            return await _dbContext.Set<JournalEntryEntity>()
                .Where(x => !x.IsDeleted && x.JournalLines.Any(l => l.AccountId == accountId && !l.IsDeleted))
                .Include(x => x.CreatedByUser)
                .Include(x => x.PostedByUser)
                .OrderByDescending(x => x.JournalDate)
                .Select(x => new JournalEntryViewModel
                {
                    JournalId = x.JournalId,
                    JournalNumber = x.JournalNumber,
                    JournalDate = x.JournalDate,
                    Description = x.Description,
                    Status = x.Status,
                    TotalDebit = x.TotalDebit,
                    TotalCredit = x.TotalCredit,
                    IsBalanced = x.IsBalanced,
                    PostedDate = x.PostedDate,
                    PostedBy = x.PostedByUser != null ? x.PostedByUser.FullName : null,
                    CreatedBy = x.CreatedByUser != null ? x.CreatedByUser.FullName : "",
                    CreatedAt = x.CreatedAt,
                    CanEdit = x.CanEdit,
                    CanDelete = x.CanDelete,
                    CanPost = x.CanPost,
                    CanReverse = x.CanReverse
                })
                .ToListAsync();
        }

        public async Task<List<JournalEntryViewModel>> GetByDateRangeAsync(DateTime dateFrom, DateTime dateTo)
        {
            return await _dbContext.Set<JournalEntryEntity>()
                .Where(x => !x.IsDeleted && x.JournalDate >= dateFrom && x.JournalDate <= dateTo)
                .Include(x => x.CreatedByUser)
                .Include(x => x.PostedByUser)
                .OrderByDescending(x => x.JournalDate)
                .Select(x => new JournalEntryViewModel
                {
                    JournalId = x.JournalId,
                    JournalNumber = x.JournalNumber,
                    JournalDate = x.JournalDate,
                    Description = x.Description,
                    Status = x.Status,
                    TotalDebit = x.TotalDebit,
                    TotalCredit = x.TotalCredit,
                    IsBalanced = x.IsBalanced,
                    PostedDate = x.PostedDate,
                    PostedBy = x.PostedByUser != null ? x.PostedByUser.FullName : null,
                    CreatedBy = x.CreatedByUser != null ? x.CreatedByUser.FullName : "",
                    CreatedAt = x.CreatedAt,
                    CanEdit = x.CanEdit,
                    CanDelete = x.CanDelete,
                    CanPost = x.CanPost,
                    CanReverse = x.CanReverse
                })
                .ToListAsync();
        }

        public async Task<byte[]> ExportToExcelAsync(DateTime? dateFrom, DateTime? dateTo, string? status, Guid? accountId)
        {
            var query = _dbContext.Set<JournalEntryEntity>()
                .Where(x => !x.IsDeleted)
                .Include(x => x.JournalLines.Where(l => !l.IsDeleted))
                    .ThenInclude(l => l.Account)
                .Include(x => x.CreatedByUser)
                .Include(x => x.PostedByUser)
                .AsQueryable();

            // Apply filters
            if (dateFrom.HasValue)
            {
                query = query.Where(x => x.JournalDate >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(x => x.JournalDate <= dateTo.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(x => x.Status == status);
            }

            if (accountId.HasValue)
            {
                query = query.Where(x => x.JournalLines.Any(l => l.AccountId == accountId.Value && !l.IsDeleted));
            }

            var data = await query.OrderByDescending(x => x.JournalDate).ToListAsync();

            using (var workbook = new NPOI.XSSF.UserModel.XSSFWorkbook())
            {
                // Header sheet
                var headerSheet = workbook.CreateSheet("Journal Headers");

                // Header style
                var headerStyle = workbook.CreateCellStyle();
                var headerFont = workbook.CreateFont();
                headerFont.IsBold = true;
                headerFont.FontHeightInPoints = 11;
                headerStyle.SetFont(headerFont);
                headerStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                headerStyle.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;

                // Create header row
                var headerRow = headerSheet.CreateRow(0);
                string[] headers = { "Journal Number", "Date", "Description", "Status", "Total Debit", "Total Credit", "Posted Date", "Posted By", "Created By", "Created At" };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = headerRow.CreateCell(i);
                    cell.SetCellValue(headers[i]);
                    cell.CellStyle = headerStyle;
                }

                // Data rows
                int rowIndex = 1;
                foreach (var item in data)
                {
                    var row = headerSheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(item.JournalNumber);
                    row.CreateCell(1).SetCellValue(item.JournalDate.ToString("yyyy-MM-dd"));
                    row.CreateCell(2).SetCellValue(item.Description);
                    row.CreateCell(3).SetCellValue(item.Status);
                    row.CreateCell(4).SetCellValue((double)item.TotalDebit);
                    row.CreateCell(5).SetCellValue((double)item.TotalCredit);
                    row.CreateCell(6).SetCellValue(item.PostedDate?.ToString("yyyy-MM-dd") ?? "");
                    row.CreateCell(7).SetCellValue(item.PostedByUser?.FullName ?? "");
                    row.CreateCell(8).SetCellValue(item.CreatedByUser?.FullName ?? "");
                    row.CreateCell(9).SetCellValue(item.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                }

                // Auto-size columns
                for (int i = 0; i < headers.Length; i++)
                {
                    headerSheet.AutoSizeColumn(i);
                }

                // Lines sheet
                var linesSheet = workbook.CreateSheet("Journal Lines");
                var linesHeaderRow = linesSheet.CreateRow(0);
                string[] linesHeaders = { "Journal Number", "Line #", "Account Code", "Account Name", "Description", "Debit", "Credit" };

                for (int i = 0; i < linesHeaders.Length; i++)
                {
                    var cell = linesHeaderRow.CreateCell(i);
                    cell.SetCellValue(linesHeaders[i]);
                    cell.CellStyle = headerStyle;
                }

                rowIndex = 1;
                foreach (var journal in data)
                {
                    foreach (var line in journal.JournalLines.OrderBy(l => l.LineNumber))
                    {
                        var row = linesSheet.CreateRow(rowIndex++);
                        row.CreateCell(0).SetCellValue(journal.JournalNumber);
                        row.CreateCell(1).SetCellValue(line.LineNumber);
                        row.CreateCell(2).SetCellValue(line.Account.AccountCode);
                        row.CreateCell(3).SetCellValue(line.Account.AccountName);
                        row.CreateCell(4).SetCellValue(line.Description ?? "");
                        row.CreateCell(5).SetCellValue((double)line.DebitAmount);
                        row.CreateCell(6).SetCellValue((double)line.CreditAmount);
                    }
                }

                // Auto-size columns
                for (int i = 0; i < linesHeaders.Length; i++)
                {
                    linesSheet.AutoSizeColumn(i);
                }

                // Write to memory stream
                using (var ms = new MemoryStream())
                {
                    workbook.Write(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
