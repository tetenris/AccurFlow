using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using AccuFlow.Infrastructures;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.ChartOfAccount;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace AccuFlow.Services
{
    public interface IChartOfAccountService : IBaseService
    {
        // CRUD Operations
        Task<BaseDatatableResponse> Datatable(DataTableChartOfAccountRequest request);
        Task<ChartOfAccountViewModel?> GetByIdAsync(Guid id);
        Task<ChartOfAccountViewModel?> GetByCodeAsync(string code);
        Task CreateAsync(CreateChartOfAccountRequest request, Guid userId);
        Task UpdateAsync(UpdateChartOfAccountRequest request, Guid userId);
        Task DeleteAsync(Guid id, Guid userId);

        // Hierarchy Operations
        Task<List<ChartOfAccountViewModel>> GetHierarchyAsync();
        Task<List<ChartOfAccountViewModel>> GetChildAccountsAsync(Guid parentId);
        Task<List<ChartOfAccountViewModel>> GetParentAccountsAsync(string accountType);
        Task<string> GetAccountPathAsync(Guid accountId);

        // Validation
        Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
        Task<bool> CanDeleteAsync(Guid accountId);
        Task<bool> HasTransactionsAsync(Guid accountId);
        Task<bool> HasChildrenAsync(Guid accountId);
        Task<bool> IsCircularReferenceAsync(Guid accountId, Guid? newParentId);

        // Status Management
        Task ToggleStatusAsync(Guid accountId, Guid userId);
        Task<List<ChartOfAccountViewModel>> GetActiveAccountsAsync();
        Task<List<ChartOfAccountViewModel>> GetDetailAccountsAsync();

        // Utility
        Task<string> GenerateAccountCodeAsync(Guid? parentId, string accountType);
        Task RecalculateHierarchyLevelsAsync();

        // Export
        Task<byte[]> ExportToExcelAsync(string? accountType, bool? isActive);
    }

    public class ChartOfAccountService : BaseService, IChartOfAccountService
    {
        private readonly ICurrentUserService _currentUserService;

        public ChartOfAccountService(AppDbContext dbContext, ICurrentUserService currentUserService) : base(dbContext)
        {
            _currentUserService = currentUserService;
        }

        // CRUD Operations
        public async Task<BaseDatatableResponse> Datatable(DataTableChartOfAccountRequest request)
        {
            var query = _dbContext.Set<ChartOfAccountEntity>()
                .Where(x => !x.IsDeleted)
                .Include(x => x.ParentAccount)
                .AsQueryable();

            var totalRecord = await query.CountAsync();

            // Search
            if (!string.IsNullOrEmpty(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x =>
                    x.AccountCode.ToLower().Contains(search) ||
                    x.AccountName.ToLower().Contains(search) ||
                    x.AccountType.ToLower().Contains(search)
                );
            }

            // Filter by Account Type
            if (!string.IsNullOrEmpty(request.AccountType))
            {
                query = query.Where(x => x.AccountType == request.AccountType);
            }

            // Filter by Status
            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            // Filter by Header
            if (request.IsHeader.HasValue)
            {
                query = query.Where(x => x.IsHeader == request.IsHeader.Value);
            }

            // Filter by Parent
            if (request.ParentAccountId.HasValue)
            {
                query = query.Where(x => x.ParentAccountId == request.ParentAccountId.Value);
            }

            // Ordering
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                query = query.OrderBy($"{request.OrderBy} {request.OrderType}");
            }
            else
            {
                query = query.OrderBy(x => x.AccountCode);
            }

            var totalFiltered = await query.CountAsync();

            // Paging
            var data = await query
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .Select(x => new ChartOfAccountViewModel
                {
                    AccountId = x.AccountId,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    AccountType = x.AccountType,
                    Description = x.Description,
                    ParentAccountId = x.ParentAccountId,
                    ParentAccountName = x.ParentAccount != null ? x.ParentAccount.AccountName : null,
                    IsHeader = x.IsHeader,
                    IsActive = x.IsActive,
                    OpeningBalance = x.OpeningBalance,
                    NormalBalance = x.NormalBalance,
                    Currency = x.Currency,
                    Level = x.Level,
                    HasChildren = x.ChildAccounts.Any(c => !c.IsDeleted),
                    ChildCount = x.ChildAccounts.Count(c => !c.IsDeleted),
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy,
                    UpdatedAt = x.UpdatedAt,
                    UpdatedBy = x.UpdatedBy
                })
                .ToListAsync();

            return new BaseDatatableResponse
            {
                Draw = request.Draw,
                RecordsTotal = totalRecord,
                RecordsFiltered = totalFiltered,
                Data = data
            };
        }

        public async Task<ChartOfAccountViewModel?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<ChartOfAccountEntity>()
                .Where(x => x.AccountId == id && !x.IsDeleted)
                .Include(x => x.ParentAccount)
                .Select(x => new ChartOfAccountViewModel
                {
                    AccountId = x.AccountId,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    AccountType = x.AccountType,
                    Description = x.Description,
                    ParentAccountId = x.ParentAccountId,
                    ParentAccountName = x.ParentAccount != null ? x.ParentAccount.AccountName : null,
                    IsHeader = x.IsHeader,
                    IsActive = x.IsActive,
                    OpeningBalance = x.OpeningBalance,
                    NormalBalance = x.NormalBalance,
                    Currency = x.Currency,
                    Level = x.Level,
                    HasChildren = x.ChildAccounts.Any(c => !c.IsDeleted),
                    ChildCount = x.ChildAccounts.Count(c => !c.IsDeleted),
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy,
                    UpdatedAt = x.UpdatedAt,
                    UpdatedBy = x.UpdatedBy
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ChartOfAccountViewModel?> GetByCodeAsync(string code)
        {
            return await _dbContext.Set<ChartOfAccountEntity>()
                .Where(x => x.AccountCode == code && !x.IsDeleted)
                .Include(x => x.ParentAccount)
                .Select(x => new ChartOfAccountViewModel
                {
                    AccountId = x.AccountId,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    AccountType = x.AccountType,
                    Description = x.Description,
                    ParentAccountId = x.ParentAccountId,
                    ParentAccountName = x.ParentAccount != null ? x.ParentAccount.AccountName : null,
                    IsHeader = x.IsHeader,
                    IsActive = x.IsActive,
                    OpeningBalance = x.OpeningBalance,
                    NormalBalance = x.NormalBalance,
                    Currency = x.Currency,
                    Level = x.Level,
                    HasChildren = x.ChildAccounts.Any(c => !c.IsDeleted),
                    ChildCount = x.ChildAccounts.Count(c => !c.IsDeleted),
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy,
                    UpdatedAt = x.UpdatedAt,
                    UpdatedBy = x.UpdatedBy
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(CreateChartOfAccountRequest request, Guid userId)
        {
            // Validate code uniqueness
            if (!await IsCodeUniqueAsync(request.AccountCode))
            {
                throw new Exception($"Account code '{request.AccountCode}' already exists");
            }

            // Validate parent account if specified
            ChartOfAccountEntity? parentAccount = null;
            int level = 0;

            if (request.ParentAccountId.HasValue)
            {
                parentAccount = await _dbContext.Set<ChartOfAccountEntity>()
                    .FirstOrDefaultAsync(x => x.AccountId == request.ParentAccountId.Value && !x.IsDeleted);

                if (parentAccount == null)
                {
                    throw new Exception("Parent account not found");
                }

                // Validate account type compatibility
                if (parentAccount.AccountType != request.AccountType)
                {
                    throw new Exception("Sub-account type must match parent account type");
                }

                level = parentAccount.Level + 1;
            }

            // Determine normal balance based on account type
            string normalBalance = GetNormalBalance(request.AccountType);

            var account = new ChartOfAccountEntity
            {
                AccountId = Guid.NewGuid(),
                AccountCode = request.AccountCode,
                AccountName = request.AccountName,
                AccountType = request.AccountType,
                Description = request.Description,
                ParentAccountId = request.ParentAccountId,
                IsHeader = request.IsHeader,
                IsActive = request.IsActive,
                OpeningBalance = request.OpeningBalance,
                NormalBalance = normalBalance,
                Currency = request.Currency,
                Level = level,
                CreatedBy = userId.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Set<ChartOfAccountEntity>().Add(account);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(UpdateChartOfAccountRequest request, Guid userId)
        {
            var account = await _dbContext.Set<ChartOfAccountEntity>()
                .FirstOrDefaultAsync(x => x.AccountId == request.AccountId && !x.IsDeleted);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            // Validate code uniqueness (excluding current account)
            if (!await IsCodeUniqueAsync(request.AccountCode, request.AccountId))
            {
                throw new Exception($"Account code '{request.AccountCode}' already exists");
            }

            // Validate parent account if changed
            if (request.ParentAccountId != account.ParentAccountId)
            {
                if (request.ParentAccountId.HasValue)
                {
                    // Check circular reference
                    if (await IsCircularReferenceAsync(request.AccountId, request.ParentAccountId.Value))
                    {
                        throw new Exception("Cannot set descendant account as parent (circular reference)");
                    }

                    var parentAccount = await _dbContext.Set<ChartOfAccountEntity>()
                        .FirstOrDefaultAsync(x => x.AccountId == request.ParentAccountId.Value && !x.IsDeleted);

                    if (parentAccount == null)
                    {
                        throw new Exception("Parent account not found");
                    }

                    // Validate account type compatibility
                    if (parentAccount.AccountType != request.AccountType)
                    {
                        throw new Exception("Sub-account type must match parent account type");
                    }

                    account.Level = parentAccount.Level + 1;
                }
                else
                {
                    account.Level = 0;
                }
            }

            account.AccountCode = request.AccountCode;
            account.AccountName = request.AccountName;
            account.AccountType = request.AccountType;
            account.Description = request.Description;
            account.ParentAccountId = request.ParentAccountId;
            account.IsHeader = request.IsHeader;
            account.IsActive = request.IsActive;
            account.OpeningBalance = request.OpeningBalance;
            account.Currency = request.Currency;
            account.NormalBalance = GetNormalBalance(request.AccountType);
            account.UpdatedBy = userId.ToString();
            account.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Recalculate hierarchy levels for children if parent changed
            if (request.ParentAccountId != account.ParentAccountId)
            {
                await RecalculateChildLevelsAsync(account.AccountId);
            }
        }

        public async Task DeleteAsync(Guid id, Guid userId)
        {
            var account = await _dbContext.Set<ChartOfAccountEntity>()
                .Include(x => x.ChildAccounts)
                .FirstOrDefaultAsync(x => x.AccountId == id && !x.IsDeleted);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            // Check if account is active
            if (account.IsActive)
            {
                throw new Exception("Cannot delete active account. Please deactivate the account first.");
            }

            // Check if account has transactions
            if (await HasTransactionsAsync(id))
            {
                throw new Exception("Cannot delete account that has been used in transactions. Consider deactivating instead.");
            }

            // Check if account has children
            if (await HasChildrenAsync(id))
            {
                throw new Exception("Cannot delete account that has sub-accounts. Delete or move sub-accounts first.");
            }

            // Soft delete
            account.IsDeleted = true;
            account.DeletedBy = userId.ToString();
            account.DeletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        // Hierarchy Operations
        public async Task<List<ChartOfAccountViewModel>> GetHierarchyAsync()
        {
            var accounts = await _dbContext.Set<ChartOfAccountEntity>()
                .Where(x => !x.IsDeleted)
                .Include(x => x.ParentAccount)
                .OrderBy(x => x.AccountCode)
                .Select(x => new ChartOfAccountViewModel
                {
                    AccountId = x.AccountId,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    AccountType = x.AccountType,
                    Description = x.Description,
                    ParentAccountId = x.ParentAccountId,
                    ParentAccountName = x.ParentAccount != null ? x.ParentAccount.AccountName : null,
                    IsHeader = x.IsHeader,
                    IsActive = x.IsActive,
                    OpeningBalance = x.OpeningBalance,
                    NormalBalance = x.NormalBalance,
                    Currency = x.Currency,
                    Level = x.Level,
                    HasChildren = x.ChildAccounts.Any(c => !c.IsDeleted),
                    ChildCount = x.ChildAccounts.Count(c => !c.IsDeleted),
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy
                })
                .ToListAsync();

            return accounts;
        }

        public async Task<List<ChartOfAccountViewModel>> GetChildAccountsAsync(Guid parentId)
        {
            return await _dbContext.Set<ChartOfAccountEntity>()
                .Where(x => x.ParentAccountId == parentId && !x.IsDeleted)
                .OrderBy(x => x.AccountCode)
                .Select(x => new ChartOfAccountViewModel
                {
                    AccountId = x.AccountId,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    AccountType = x.AccountType,
                    Description = x.Description,
                    ParentAccountId = x.ParentAccountId,
                    IsHeader = x.IsHeader,
                    IsActive = x.IsActive,
                    Level = x.Level,
                    HasChildren = x.ChildAccounts.Any(c => !c.IsDeleted),
                    ChildCount = x.ChildAccounts.Count(c => !c.IsDeleted)
                })
                .ToListAsync();
        }

        public async Task<List<ChartOfAccountViewModel>> GetParentAccountsAsync(string accountType)
        {
            return await _dbContext.Set<ChartOfAccountEntity>()
                .Where(x => x.AccountType == accountType && !x.IsDeleted && x.IsActive)
                .OrderBy(x => x.AccountCode)
                .Select(x => new ChartOfAccountViewModel
                {
                    AccountId = x.AccountId,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    AccountType = x.AccountType,
                    Level = x.Level
                })
                .ToListAsync();
        }

        public async Task<string> GetAccountPathAsync(Guid accountId)
        {
            var account = await _dbContext.Set<ChartOfAccountEntity>()
                .Include(x => x.ParentAccount)
                .FirstOrDefaultAsync(x => x.AccountId == accountId && !x.IsDeleted);

            if (account == null)
                return string.Empty;

            var path = new List<string> { account.AccountName };
            var currentParent = account.ParentAccount;

            while (currentParent != null)
            {
                path.Insert(0, currentParent.AccountName);
                currentParent = await _dbContext.Set<ChartOfAccountEntity>()
                    .Include(x => x.ParentAccount)
                    .Where(x => x.AccountId == currentParent.ParentAccountId && !x.IsDeleted)
                    .Select(x => x.ParentAccount)
                    .FirstOrDefaultAsync();
            }

            return string.Join(" > ", path);
        }

        // Validation
        public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
        {
            var query = _dbContext.Set<ChartOfAccountEntity>()
                .Where(x => x.AccountCode == code && !x.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.AccountId != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<bool> CanDeleteAsync(Guid accountId)
        {
            if (await HasTransactionsAsync(accountId))
                return false;

            if (await HasChildrenAsync(accountId))
                return false;

            return true;
        }

        public async Task<bool> HasTransactionsAsync(Guid accountId)
        {
            // TODO: Implement when journal entry module is created
            // Check if account is used in any journal entries
            return false;
        }

        public async Task<bool> HasChildrenAsync(Guid accountId)
        {
            return await _dbContext.Set<ChartOfAccountEntity>()
                .AnyAsync(x => x.ParentAccountId == accountId && !x.IsDeleted);
        }

        public async Task<bool> IsCircularReferenceAsync(Guid accountId, Guid? newParentId)
        {
            if (!newParentId.HasValue)
                return false;

            // Check if newParentId is a descendant of accountId
            var descendants = await GetAllDescendantsAsync(accountId);
            return descendants.Any(x => x.AccountId == newParentId.Value);
        }

        // Status Management
        public async Task ToggleStatusAsync(Guid accountId, Guid userId)
        {
            var account = await _dbContext.Set<ChartOfAccountEntity>()
                .FirstOrDefaultAsync(x => x.AccountId == accountId && !x.IsDeleted);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            account.IsActive = !account.IsActive;
            account.UpdatedBy = userId.ToString();
            account.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ChartOfAccountViewModel>> GetActiveAccountsAsync()
        {
            return await _dbContext.Set<ChartOfAccountEntity>()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.AccountCode)
                .Select(x => new ChartOfAccountViewModel
                {
                    AccountId = x.AccountId,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    AccountType = x.AccountType,
                    Level = x.Level,
                    IsHeader = x.IsHeader
                })
                .ToListAsync();
        }

        public async Task<List<ChartOfAccountViewModel>> GetDetailAccountsAsync()
        {
            return await _dbContext.Set<ChartOfAccountEntity>()
                .Where(x => !x.IsHeader && x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.AccountCode)
                .Select(x => new ChartOfAccountViewModel
                {
                    AccountId = x.AccountId,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    AccountType = x.AccountType,
                    IsActive = x.IsActive,
                    Level = x.Level
                })
                .ToListAsync();
        }

        // Utility
        public async Task<string> GenerateAccountCodeAsync(Guid? parentId, string accountType)
        {
            string prefix = GetAccountTypePrefix(accountType);

            if (parentId.HasValue)
            {
                var parent = await _dbContext.Set<ChartOfAccountEntity>()
                    .FirstOrDefaultAsync(x => x.AccountId == parentId.Value && !x.IsDeleted);

                if (parent != null)
                {
                    // Get last child code
                    var lastChild = await _dbContext.Set<ChartOfAccountEntity>()
                        .Where(x => x.ParentAccountId == parentId.Value && !x.IsDeleted)
                        .OrderByDescending(x => x.AccountCode)
                        .FirstOrDefaultAsync();

                    if (lastChild != null)
                    {
                        // Increment last child code
                        var parts = lastChild.AccountCode.Split('-');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int number))
                        {
                            return $"{prefix}-{(number + 100):D5}";
                        }
                    }

                    // First child
                    var parentParts = parent.AccountCode.Split('-');
                    if (parentParts.Length == 2 && int.TryParse(parentParts[1], out int parentNumber))
                    {
                        return $"{prefix}-{(parentNumber + 100):D5}";
                    }
                }
            }

            // Root account - find last root account of this type
            var lastRoot = await _dbContext.Set<ChartOfAccountEntity>()
                .Where(x => x.ParentAccountId == null && x.AccountType == accountType && !x.IsDeleted)
                .OrderByDescending(x => x.AccountCode)
                .FirstOrDefaultAsync();

            if (lastRoot != null)
            {
                var parts = lastRoot.AccountCode.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out int number))
                {
                    return $"{prefix}-{(number + 10000):D5}";
                }
            }

            // First account of this type
            return $"{prefix}-10000";
        }

        public async Task RecalculateHierarchyLevelsAsync()
        {
            var allAccounts = await _dbContext.Set<ChartOfAccountEntity>()
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            foreach (var account in allAccounts)
            {
                account.Level = await CalculateLevelAsync(account.AccountId);
            }

            await _dbContext.SaveChangesAsync();
        }

        // Private Helper Methods
        private string GetNormalBalance(string accountType)
        {
            return accountType switch
            {
                "Asset" => "Debit",
                "Expense" => "Debit",
                "Other Expense" => "Debit",
                "Liability" => "Credit",
                "Equity" => "Credit",
                "Revenue" => "Credit",
                "Other Income" => "Credit",
                _ => "Debit"
            };
        }

        private string GetAccountTypePrefix(string accountType)
        {
            return accountType switch
            {
                "Asset" => "1",
                "Liability" => "2",
                "Equity" => "3",
                "Revenue" => "4",
                "Expense" => "5",
                "Other Income" => "6",
                "Other Expense" => "7",
                _ => "1"
            };
        }

        private async Task<int> CalculateLevelAsync(Guid accountId)
        {
            var account = await _dbContext.Set<ChartOfAccountEntity>()
                .FirstOrDefaultAsync(x => x.AccountId == accountId && !x.IsDeleted);

            if (account == null || !account.ParentAccountId.HasValue)
                return 0;

            return 1 + await CalculateLevelAsync(account.ParentAccountId.Value);
        }

        private async Task RecalculateChildLevelsAsync(Guid parentId)
        {
            var children = await _dbContext.Set<ChartOfAccountEntity>()
                .Where(x => x.ParentAccountId == parentId && !x.IsDeleted)
                .ToListAsync();

            foreach (var child in children)
            {
                child.Level = await CalculateLevelAsync(child.AccountId);
                await RecalculateChildLevelsAsync(child.AccountId);
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task<List<ChartOfAccountEntity>> GetAllDescendantsAsync(Guid accountId)
        {
            var descendants = new List<ChartOfAccountEntity>();
            var children = await _dbContext.Set<ChartOfAccountEntity>()
                .Where(x => x.ParentAccountId == accountId && !x.IsDeleted)
                .ToListAsync();

            descendants.AddRange(children);

            foreach (var child in children)
            {
                var childDescendants = await GetAllDescendantsAsync(child.AccountId);
                descendants.AddRange(childDescendants);
            }

            return descendants;
        }

        public async Task<byte[]> ExportToExcelAsync(string? accountType, bool? isActive)
        {
            var query = _dbContext.Set<ChartOfAccountEntity>()
                .Where(x => !x.IsDeleted)
                .Include(x => x.ParentAccount)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(accountType))
            {
                query = query.Where(x => x.AccountType == accountType);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var data = await query
                .OrderBy(x => x.AccountCode)
                .Select(x => new
                {
                    x.AccountCode,
                    x.AccountName,
                    x.AccountType,
                    ParentAccountName = x.ParentAccount != null ? x.ParentAccount.AccountName : "-",
                    x.Description,
                    x.NormalBalance,
                    x.OpeningBalance,
                    x.Currency,
                    IsHeader = x.IsHeader ? "Yes" : "No",
                    IsActive = x.IsActive ? "Active" : "Inactive",
                    x.Level
                })
                .ToListAsync();

            using (var workbook = new NPOI.XSSF.UserModel.XSSFWorkbook())
            {
                var sheet = workbook.CreateSheet("Chart of Accounts");

                // Header style
                var headerStyle = workbook.CreateCellStyle();
                var headerFont = workbook.CreateFont();
                headerFont.IsBold = true;
                headerFont.FontHeightInPoints = 11;
                headerStyle.SetFont(headerFont);
                headerStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                headerStyle.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;
                headerStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                headerStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                headerStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                headerStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;

                // Create header row
                var headerRow = sheet.CreateRow(0);
                string[] headers = { "Account Code", "Account Name", "Type", "Parent Account", "Description", "Normal Balance", "Opening Balance", "Currency", "Is Header", "Status", "Level" };
                
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
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(item.AccountCode);
                    row.CreateCell(1).SetCellValue(item.AccountName);
                    row.CreateCell(2).SetCellValue(item.AccountType);
                    row.CreateCell(3).SetCellValue(item.ParentAccountName);
                    row.CreateCell(4).SetCellValue(item.Description ?? "");
                    row.CreateCell(5).SetCellValue(item.NormalBalance);
                    row.CreateCell(6).SetCellValue((double)item.OpeningBalance);
                    row.CreateCell(7).SetCellValue(item.Currency ?? "");
                    row.CreateCell(8).SetCellValue(item.IsHeader);
                    row.CreateCell(9).SetCellValue(item.IsActive);
                    row.CreateCell(10).SetCellValue(item.Level);
                }

                // Auto-size columns
                for (int i = 0; i < headers.Length; i++)
                {
                    sheet.AutoSizeColumn(i);
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
