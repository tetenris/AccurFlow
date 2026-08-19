using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ChartOfAccounts.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.ChartOfAccount;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ChartOfAccounts.Handlers
{
    public class ImportCoaCommandHandler : IRequestHandler<ImportCoaCommand, ImportChartOfAccountResult>
    {
        private static readonly string[] TemplateHeaders =
        {
            "Account Code",
            "Account Name",
            "Account Type",
            "Parent Account Code",
            "Description",
            "Is Header",
            "Is Active",
            "Opening Balance",
            "Currency"
        };

        private readonly IRepository<ChartOfAccountEntity> _coaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ImportCoaCommandHandler(
            IRepository<ChartOfAccountEntity> coaRepository,
            IUnitOfWork unitOfWork)
        {
            _coaRepository = coaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ImportChartOfAccountResult> Handle(ImportCoaCommand request, CancellationToken cancellationToken)
        {
            var file = request.File;
            var userId = request.UserId;

            if (file == null || file.Length == 0)
                throw new Exception("Please select a file to import");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xlsx")
                throw new Exception("Only .xlsx files are supported. Please download the template and fill it in.");

            var result = new ImportChartOfAccountResult();

            using (var stream = file.OpenReadStream())
            using (var workbook = new NPOI.XSSF.UserModel.XSSFWorkbook(stream))
            {
                var sheet = workbook.GetSheetAt(0);
                if (sheet == null)
                    throw new Exception("The Excel file does not contain any worksheet");

                var headerRow = sheet.GetRow(0);
                if (headerRow == null)
                    throw new Exception("The file has no header row. Please use the downloaded template.");

                var headerColumnCount = headerRow.LastCellNum;
                if (headerColumnCount != TemplateHeaders.Length)
                    throw new Exception($"Invalid number of columns. Expected {TemplateHeaders.Length} columns, found {headerColumnCount}. Please use the downloaded template.");

                for (int i = 0; i < TemplateHeaders.Length; i++)
                {
                    var headerCell = headerRow.GetCell(i);
                    var headerValue = headerCell?.ToString()?.Trim() ?? string.Empty;
                    if (!string.Equals(headerValue, TemplateHeaders[i], StringComparison.OrdinalIgnoreCase))
                        throw new Exception($"Invalid header at column {i + 1}. Expected '{TemplateHeaders[i]}', found '{headerValue}'. Header names must match the template exactly.");
                }

                var accountTypes = new[] { "Asset", "Liability", "Equity", "Revenue", "Expense", "Other Income", "Other Expense" };

                var existingAccounts = await _coaRepository.Query()
                    .Where(x => !x.IsDeleted)
                    .ToListAsync(cancellationToken);

                var codeToAccount = existingAccounts.ToDictionary(x => x.AccountCode, x => x.AccountId);

                var accountsToAdd = new List<ChartOfAccountEntity>();
                var currentRow = 1;

                while (true)
                {
                    var row = sheet.GetRow(currentRow);
                    if (row == null) break;

                    bool isEmptyRow = true;
                    for (int c = 0; c < TemplateHeaders.Length; c++)
                    {
                        var cell = row.GetCell(c);
                        if (cell != null && !string.IsNullOrWhiteSpace(cell.ToString()))
                        {
                            isEmptyRow = false;
                            break;
                        }
                    }
                    if (isEmptyRow) break;

                    result.TotalRows++;

                    var getValue = (int col) => row.GetCell(col)?.ToString()?.Trim() ?? string.Empty;

                    var accountCode = getValue(0);
                    var accountName = getValue(1);
                    var accountType = getValue(2);
                    var parentCode = getValue(3);
                    var description = getValue(4);
                    var isHeaderText = getValue(5);
                    var isActiveText = getValue(6);
                    var openingBalanceText = getValue(7);
                    var currency = getValue(8);

                    var rowErrors = new List<string>();
                    if (string.IsNullOrWhiteSpace(accountCode))
                        rowErrors.Add("Account Code is required");
                    else if (accountCode.Length > 20)
                        rowErrors.Add("Account Code cannot exceed 20 characters");
                    else if (codeToAccount.ContainsKey(accountCode))
                        rowErrors.Add($"Account Code '{accountCode}' already exists");

                    if (string.IsNullOrWhiteSpace(accountName))
                        rowErrors.Add("Account Name is required");
                    else if (accountName.Length < 3 || accountName.Length > 255)
                        rowErrors.Add("Account Name must be between 3 and 255 characters");

                    if (string.IsNullOrWhiteSpace(accountType))
                        rowErrors.Add("Account Type is required");
                    else if (!accountTypes.Contains(accountType))
                        rowErrors.Add($"Account Type '{accountType}' is not valid. Must be one of: {string.Join(", ", accountTypes)}");

                    bool isHeader = false;
                    if (string.IsNullOrWhiteSpace(isHeaderText))
                        rowErrors.Add("Is Header is required (Yes/No)");
                    else if (!ChartOfAccountHelper.TryParseYesNo(isHeaderText, out isHeader))
                        rowErrors.Add($"Is Header value '{isHeaderText}' is not valid. Use Yes or No.");

                    bool isActive = true;
                    if (string.IsNullOrWhiteSpace(isActiveText))
                        rowErrors.Add("Is Active is required (Yes/No)");
                    else if (!ChartOfAccountHelper.TryParseYesNo(isActiveText, out isActive))
                        rowErrors.Add($"Is Active value '{isActiveText}' is not valid. Use Yes or No.");

                    decimal openingBalance = 0;
                    if (!string.IsNullOrWhiteSpace(openingBalanceText) && !decimal.TryParse(openingBalanceText, out openingBalance))
                        rowErrors.Add($"Opening Balance value '{openingBalanceText}' is not a valid number");

                    Guid? parentAccountId = null;
                    if (!string.IsNullOrWhiteSpace(parentCode))
                    {
                        if (codeToAccount.TryGetValue(parentCode, out var existingParentId))
                        {
                            parentAccountId = existingParentId;
                            var parent = existingAccounts.FirstOrDefault(x => x.AccountId == existingParentId)
                                ?? accountsToAdd.FirstOrDefault(x => x.AccountId == existingParentId);
                            if (parent != null && string.Equals(parent.AccountType, accountType, StringComparison.OrdinalIgnoreCase) == false)
                                rowErrors.Add($"Parent account '{parentCode}' type ({parent.AccountType}) does not match '{accountType}'");
                        }
                        else
                        {
                            rowErrors.Add($"Parent account code '{parentCode}' not found. Make sure parent exists in the system or is listed earlier in the file.");
                        }
                    }

                    if (rowErrors.Any())
                    {
                        result.SkippedCount++;
                        result.Errors.Add($"Row {currentRow + 1}: {string.Join("; ", rowErrors)}");
                        currentRow++;
                        continue;
                    }

                    var account = new ChartOfAccountEntity
                    {
                        AccountId = Guid.NewGuid(),
                        AccountCode = accountCode,
                        AccountName = accountName,
                        AccountType = accountType,
                        Description = description,
                        ParentAccountId = parentAccountId,
                        IsHeader = isHeader,
                        IsActive = isActive,
                        OpeningBalance = openingBalance,
                        NormalBalance = ChartOfAccountHelper.GetNormalBalance(accountType),
                        Currency = string.IsNullOrWhiteSpace(currency) ? "IDR" : currency,
                        Level = parentAccountId.HasValue
                            ? (existingAccounts.FirstOrDefault(x => x.AccountId == parentAccountId.Value)?.Level ?? -1) + 1
                            : 0,
                        CreatedBy = userId.ToString(),
                        CreatedAt = DateTime.UtcNow
                    };

                    codeToAccount[accountCode] = account.AccountId;
                    existingAccounts.Add(account);
                    accountsToAdd.Add(account);

                    currentRow++;
                }

                if (accountsToAdd.Any())
                {
                    _coaRepository.AddRange(accountsToAdd);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                result.ImportedCount = accountsToAdd.Count;
                result.Success = true;

                if (result.SkippedCount == 0)
                    result.Message = $"Successfully imported {result.ImportedCount} account(s)";
                else
                    result.Message = $"Imported {result.ImportedCount} account(s), skipped {result.SkippedCount} row(s) with errors";

                return result;
            }
        }
    }
}