using AccuFlow.Infrastructure.Persistence;
using AccuFlow.Domain.Entities;
using AccuFlow.Services;
using AccuFlow.Models.FinancialStatement;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace AccuFlow.Services;

public class FinancialStatementService : BaseService, IFinancialStatementService
{
    private readonly IGeneralLedgerService _generalLedgerService;

    public FinancialStatementService(
        AppDbContext dbContext,
        IGeneralLedgerService generalLedgerService) : base(dbContext)
    {
        _generalLedgerService = generalLedgerService;
    }

    public async Task<IncomeStatementViewModel> GetIncomeStatementAsync(GetIncomeStatementRequest request)
    {
        var accounts = await _dbContext.Set<ChartOfAccountEntity>()
            .Where(a => !a.IsDeleted && a.IsActive && !a.IsHeader
                && (a.AccountType == "Revenue" || a.AccountType == "Other Income"
                    || a.AccountType == "Expense" || a.AccountType == "Other Expense"))
            .OrderBy(a => a.AccountCode)
            .ToListAsync();

        var sections = new List<IncomeStatementSectionViewModel>();
        var accountTypes = new[] { "Revenue", "Other Income", "Expense", "Other Expense" };

        foreach (var accountType in accountTypes)
        {
            var accountsInType = accounts.Where(a => a.AccountType == accountType).ToList();
            var lines = new List<IncomeStatementLineViewModel>();

            foreach (var account in accountsInType)
            {
                var balance = await _generalLedgerService.GetAccountBalanceAsync(account.AccountId, request.DateTo);

                if (!request.ShowZeroBalance && balance == 0)
                    continue;

                lines.Add(new IncomeStatementLineViewModel
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountCode,
                    AccountName = account.AccountName,
                    Amount = Math.Abs(balance)
                });
            }

            if (lines.Any())
            {
                sections.Add(new IncomeStatementSectionViewModel
                {
                    SectionName = accountType,
                    Lines = lines,
                    Subtotal = lines.Sum(l => l.Amount)
                });
            }
        }

        var totalRevenue = sections.Where(s => s.SectionName == "Revenue" || s.SectionName == "Other Income")
            .Sum(s => s.Subtotal);
        var totalExpense = sections.Where(s => s.SectionName == "Expense" || s.SectionName == "Other Expense")
            .Sum(s => s.Subtotal);

        return new IncomeStatementViewModel
        {
            DateFrom = request.DateFrom,
            DateTo = request.DateTo,
            Sections = sections,
            TotalRevenue = totalRevenue,
            TotalExpense = totalExpense,
            NetIncome = totalRevenue - totalExpense
        };
    }

    public async Task<BalanceSheetViewModel> GetBalanceSheetAsync(GetBalanceSheetRequest request)
    {
        var accounts = await _dbContext.Set<ChartOfAccountEntity>()
            .Where(a => !a.IsDeleted && a.IsActive && !a.IsHeader
                && (a.AccountType == "Asset" || a.AccountType == "Liability" || a.AccountType == "Equity"))
            .OrderBy(a => a.AccountCode)
            .ToListAsync();

        var sections = new List<BalanceSheetSectionViewModel>();
        var accountTypes = new[] { "Asset", "Liability", "Equity" };

        foreach (var accountType in accountTypes)
        {
            var accountsInType = accounts.Where(a => a.AccountType == accountType).ToList();
            var lines = new List<BalanceSheetLineViewModel>();

            foreach (var account in accountsInType)
            {
                var balance = await _generalLedgerService.GetAccountBalanceAsync(account.AccountId, request.AsOfDate);

                if (!request.ShowZeroBalance && balance == 0)
                    continue;

                lines.Add(new BalanceSheetLineViewModel
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountCode,
                    AccountName = account.AccountName,
                    Amount = Math.Abs(balance)
                });
            }

            if (lines.Any())
            {
                sections.Add(new BalanceSheetSectionViewModel
                {
                    SectionName = accountType,
                    Lines = lines,
                    Subtotal = lines.Sum(l => l.Amount)
                });
            }
        }

        var totalAssets = sections.Where(s => s.SectionName == "Asset").Sum(s => s.Subtotal);
        var totalLiabilities = sections.Where(s => s.SectionName == "Liability").Sum(s => s.Subtotal);
        var totalEquity = sections.Where(s => s.SectionName == "Equity").Sum(s => s.Subtotal);
        var difference = totalAssets - (totalLiabilities + totalEquity);

        return new BalanceSheetViewModel
        {
            AsOfDate = request.AsOfDate,
            Sections = sections,
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities,
            TotalEquity = totalEquity,
            IsBalanced = Math.Abs(difference) < 0.01m,
            Difference = difference
        };
    }

    public async Task<CashFlowStatementViewModel> GetCashFlowStatementAsync(GetCashFlowStatementRequest request)
    {
        // Get cash accounts (accounts with "Cash" or "Bank" in name or specific codes starting with 1-1)
        var cashAccounts = await _dbContext.Set<ChartOfAccountEntity>()
            .Where(a => !a.IsDeleted && a.IsActive && !a.IsHeader
                && (a.AccountName.Contains("Cash") || a.AccountName.Contains("Bank") || a.AccountCode.StartsWith("1-1")))
            .ToListAsync();

        decimal beginningBalance = 0;
        decimal endingBalance = 0;

        foreach (var account in cashAccounts)
        {
            beginningBalance += await _generalLedgerService.GetOpeningBalanceAsync(account.AccountId, request.DateFrom);
            endingBalance += await _generalLedgerService.GetAccountBalanceAsync(account.AccountId, request.DateTo);
        }

        // For simplified cash flow, we'll show the net change categorized by account type
        var sections = new List<CashFlowSectionViewModel>();

        // Operating Activities (Revenue and Expense accounts)
        var operatingAccounts = await _dbContext.Set<ChartOfAccountEntity>()
            .Where(a => !a.IsDeleted && a.IsActive && !a.IsHeader
                && (a.AccountType == "Revenue" || a.AccountType == "Expense"))
            .ToListAsync();

        var operatingLines = new List<CashFlowLineViewModel>();
        foreach (var account in operatingAccounts)
        {
            var balance = await _generalLedgerService.GetAccountBalanceAsync(account.AccountId, request.DateTo);
            if (balance != 0)
            {
                operatingLines.Add(new CashFlowLineViewModel
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountCode,
                    AccountName = account.AccountName,
                    Amount = account.AccountType == "Revenue" ? balance : -balance
                });
            }
        }

        if (operatingLines.Any())
        {
            sections.Add(new CashFlowSectionViewModel
            {
                SectionName = "Operating Activities",
                Lines = operatingLines,
                Subtotal = operatingLines.Sum(l => l.Amount)
            });
        }

        // Investing Activities (Asset accounts except cash)
        var investingAccounts = await _dbContext.Set<ChartOfAccountEntity>()
            .Where(a => !a.IsDeleted && a.IsActive && !a.IsHeader
                && a.AccountType == "Asset"
                && !a.AccountName.Contains("Cash") && !a.AccountName.Contains("Bank"))
            .ToListAsync();

        var investingLines = new List<CashFlowLineViewModel>();
        foreach (var account in investingAccounts)
        {
            var balance = await _generalLedgerService.GetAccountBalanceAsync(account.AccountId, request.DateTo);
            if (balance != 0)
            {
                investingLines.Add(new CashFlowLineViewModel
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountCode,
                    AccountName = account.AccountName,
                    Amount = -balance
                });
            }
        }

        if (investingLines.Any())
        {
            sections.Add(new CashFlowSectionViewModel
            {
                SectionName = "Investing Activities",
                Lines = investingLines,
                Subtotal = investingLines.Sum(l => l.Amount)
            });
        }

        // Financing Activities (Liability and Equity accounts)
        var financingAccounts = await _dbContext.Set<ChartOfAccountEntity>()
            .Where(a => !a.IsDeleted && a.IsActive && !a.IsHeader
                && (a.AccountType == "Liability" || a.AccountType == "Equity"))
            .ToListAsync();

        var financingLines = new List<CashFlowLineViewModel>();
        foreach (var account in financingAccounts)
        {
            var balance = await _generalLedgerService.GetAccountBalanceAsync(account.AccountId, request.DateTo);
            if (balance != 0)
            {
                financingLines.Add(new CashFlowLineViewModel
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountCode,
                    AccountName = account.AccountName,
                    Amount = balance
                });
            }
        }

        if (financingLines.Any())
        {
            sections.Add(new CashFlowSectionViewModel
            {
                SectionName = "Financing Activities",
                Lines = financingLines,
                Subtotal = financingLines.Sum(l => l.Amount)
            });
        }

        var netOperating = sections.FirstOrDefault(s => s.SectionName == "Operating Activities")?.Subtotal ?? 0;
        var netInvesting = sections.FirstOrDefault(s => s.SectionName == "Investing Activities")?.Subtotal ?? 0;
        var netFinancing = sections.FirstOrDefault(s => s.SectionName == "Financing Activities")?.Subtotal ?? 0;
        var netChange = endingBalance - beginningBalance;

        return new CashFlowStatementViewModel
        {
            DateFrom = request.DateFrom,
            DateTo = request.DateTo,
            BeginningCashBalance = beginningBalance,
            Sections = sections,
            NetCashFromOperating = netOperating,
            NetCashFromInvesting = netInvesting,
            NetCashFromFinancing = netFinancing,
            NetIncreaseDecrease = netChange,
            EndingCashBalance = endingBalance
        };
    }

    public async Task<byte[]> ExportIncomeStatementAsync(GetIncomeStatementRequest request)
    {
        var incomeStatement = await GetIncomeStatementAsync(request);

        var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("Income Statement");

        var headerStyle = workbook.CreateCellStyle();
        var headerFont = workbook.CreateFont();
        headerFont.IsBold = true;
        headerStyle.SetFont(headerFont);

        int rowIndex = 0;

        var titleRow = sheet.CreateRow(rowIndex++);
        var titleCell = titleRow.CreateCell(0);
        titleCell.SetCellValue("Income Statement");
        titleCell.CellStyle = headerStyle;

        var dateRow = sheet.CreateRow(rowIndex++);
        dateRow.CreateCell(0).SetCellValue($"Period: {incomeStatement.DateFrom:dd/MM/yyyy} - {incomeStatement.DateTo:dd/MM/yyyy}");

        rowIndex++;

        var headerRow = sheet.CreateRow(rowIndex++);
        headerRow.CreateCell(0).SetCellValue("Account Code");
        headerRow.CreateCell(1).SetCellValue("Account Name");
        headerRow.CreateCell(2).SetCellValue("Amount");
        for (int i = 0; i < 3; i++)
        {
            headerRow.GetCell(i).CellStyle = headerStyle;
        }

        foreach (var section in incomeStatement.Sections)
        {
            var sectionRow = sheet.CreateRow(rowIndex++);
            sectionRow.CreateCell(0).SetCellValue(section.SectionName);
            sectionRow.GetCell(0).CellStyle = headerStyle;

            foreach (var line in section.Lines)
            {
                var row = sheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(line.AccountCode);
                row.CreateCell(1).SetCellValue(line.AccountName);
                row.CreateCell(2).SetCellValue((double)line.Amount);
            }

            var subtotalRow = sheet.CreateRow(rowIndex++);
            subtotalRow.CreateCell(1).SetCellValue($"Total {section.SectionName}");
            subtotalRow.CreateCell(2).SetCellValue((double)section.Subtotal);
            for (int i = 1; i < 3; i++)
            {
                subtotalRow.GetCell(i).CellStyle = headerStyle;
            }

            rowIndex++;
        }

        var totalRevenueRow = sheet.CreateRow(rowIndex++);
        totalRevenueRow.CreateCell(1).SetCellValue("TOTAL REVENUE");
        totalRevenueRow.CreateCell(2).SetCellValue((double)incomeStatement.TotalRevenue);
        for (int i = 1; i < 3; i++)
        {
            totalRevenueRow.GetCell(i).CellStyle = headerStyle;
        }

        var totalExpenseRow = sheet.CreateRow(rowIndex++);
        totalExpenseRow.CreateCell(1).SetCellValue("TOTAL EXPENSE");
        totalExpenseRow.CreateCell(2).SetCellValue((double)incomeStatement.TotalExpense);
        for (int i = 1; i < 3; i++)
        {
            totalExpenseRow.GetCell(i).CellStyle = headerStyle;
        }

        rowIndex++;
        var netIncomeRow = sheet.CreateRow(rowIndex++);
        netIncomeRow.CreateCell(1).SetCellValue("NET INCOME");
        netIncomeRow.CreateCell(2).SetCellValue((double)incomeStatement.NetIncome);
        for (int i = 1; i < 3; i++)
        {
            netIncomeRow.GetCell(i).CellStyle = headerStyle;
        }

        for (int i = 0; i < 3; i++)
        {
            sheet.AutoSizeColumn(i);
        }

        using var ms = new MemoryStream();
        workbook.Write(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportBalanceSheetAsync(GetBalanceSheetRequest request)
    {
        var balanceSheet = await GetBalanceSheetAsync(request);

        var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("Balance Sheet");

        var headerStyle = workbook.CreateCellStyle();
        var headerFont = workbook.CreateFont();
        headerFont.IsBold = true;
        headerStyle.SetFont(headerFont);

        int rowIndex = 0;

        var titleRow = sheet.CreateRow(rowIndex++);
        var titleCell = titleRow.CreateCell(0);
        titleCell.SetCellValue("Balance Sheet");
        titleCell.CellStyle = headerStyle;

        var dateRow = sheet.CreateRow(rowIndex++);
        dateRow.CreateCell(0).SetCellValue($"As of: {balanceSheet.AsOfDate:dd/MM/yyyy}");

        rowIndex++;

        var headerRow = sheet.CreateRow(rowIndex++);
        headerRow.CreateCell(0).SetCellValue("Account Code");
        headerRow.CreateCell(1).SetCellValue("Account Name");
        headerRow.CreateCell(2).SetCellValue("Amount");
        for (int i = 0; i < 3; i++)
        {
            headerRow.GetCell(i).CellStyle = headerStyle;
        }

        foreach (var section in balanceSheet.Sections)
        {
            var sectionRow = sheet.CreateRow(rowIndex++);
            sectionRow.CreateCell(0).SetCellValue(section.SectionName);
            sectionRow.GetCell(0).CellStyle = headerStyle;

            foreach (var line in section.Lines)
            {
                var row = sheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(line.AccountCode);
                row.CreateCell(1).SetCellValue(line.AccountName);
                row.CreateCell(2).SetCellValue((double)line.Amount);
            }

            var subtotalRow = sheet.CreateRow(rowIndex++);
            subtotalRow.CreateCell(1).SetCellValue($"Total {section.SectionName}");
            subtotalRow.CreateCell(2).SetCellValue((double)section.Subtotal);
            for (int i = 1; i < 3; i++)
            {
                subtotalRow.GetCell(i).CellStyle = headerStyle;
            }

            rowIndex++;
        }

        var totalLiabEquityRow = sheet.CreateRow(rowIndex++);
        totalLiabEquityRow.CreateCell(1).SetCellValue("TOTAL LIABILITIES + EQUITY");
        totalLiabEquityRow.CreateCell(2).SetCellValue((double)(balanceSheet.TotalLiabilities + balanceSheet.TotalEquity));
        for (int i = 1; i < 3; i++)
        {
            totalLiabEquityRow.GetCell(i).CellStyle = headerStyle;
        }

        if (!balanceSheet.IsBalanced)
        {
            var diffRow = sheet.CreateRow(rowIndex++);
            diffRow.CreateCell(1).SetCellValue("Difference");
            diffRow.CreateCell(2).SetCellValue((double)Math.Abs(balanceSheet.Difference));
        }

        for (int i = 0; i < 3; i++)
        {
            sheet.AutoSizeColumn(i);
        }

        using var ms = new MemoryStream();
        workbook.Write(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportCashFlowAsync(GetCashFlowStatementRequest request)
    {
        var cashFlow = await GetCashFlowStatementAsync(request);

        var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("Cash Flow");

        var headerStyle = workbook.CreateCellStyle();
        var headerFont = workbook.CreateFont();
        headerFont.IsBold = true;
        headerStyle.SetFont(headerFont);

        int rowIndex = 0;

        var titleRow = sheet.CreateRow(rowIndex++);
        var titleCell = titleRow.CreateCell(0);
        titleCell.SetCellValue("Cash Flow Statement");
        titleCell.CellStyle = headerStyle;

        var dateRow = sheet.CreateRow(rowIndex++);
        dateRow.CreateCell(0).SetCellValue($"Period: {cashFlow.DateFrom:dd/MM/yyyy} - {cashFlow.DateTo:dd/MM/yyyy}");

        rowIndex++;

        var beginningRow = sheet.CreateRow(rowIndex++);
        beginningRow.CreateCell(0).SetCellValue("Beginning Cash Balance");
        beginningRow.CreateCell(2).SetCellValue((double)cashFlow.BeginningCashBalance);
        beginningRow.GetCell(0).CellStyle = headerStyle;

        rowIndex++;

        var headerRow = sheet.CreateRow(rowIndex++);
        headerRow.CreateCell(0).SetCellValue("Account Code");
        headerRow.CreateCell(1).SetCellValue("Account Name");
        headerRow.CreateCell(2).SetCellValue("Amount");
        for (int i = 0; i < 3; i++)
        {
            headerRow.GetCell(i).CellStyle = headerStyle;
        }

        foreach (var section in cashFlow.Sections)
        {
            var sectionRow = sheet.CreateRow(rowIndex++);
            sectionRow.CreateCell(0).SetCellValue(section.SectionName);
            sectionRow.GetCell(0).CellStyle = headerStyle;

            foreach (var line in section.Lines)
            {
                var row = sheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(line.AccountCode);
                row.CreateCell(1).SetCellValue(line.AccountName);
                row.CreateCell(2).SetCellValue((double)line.Amount);
            }

            var subtotalRow = sheet.CreateRow(rowIndex++);
            subtotalRow.CreateCell(1).SetCellValue($"Net Cash from {section.SectionName}");
            subtotalRow.CreateCell(2).SetCellValue((double)section.Subtotal);
            for (int i = 1; i < 3; i++)
            {
                subtotalRow.GetCell(i).CellStyle = headerStyle;
            }

            rowIndex++;
        }

        var netChangeRow = sheet.CreateRow(rowIndex++);
        netChangeRow.CreateCell(0).SetCellValue("Net Increase (Decrease) in Cash");
        netChangeRow.CreateCell(2).SetCellValue((double)cashFlow.NetIncreaseDecrease);
        netChangeRow.GetCell(0).CellStyle = headerStyle;

        rowIndex++;

        var endingRow = sheet.CreateRow(rowIndex++);
        endingRow.CreateCell(0).SetCellValue("Ending Cash Balance");
        endingRow.CreateCell(2).SetCellValue((double)cashFlow.EndingCashBalance);
        endingRow.GetCell(0).CellStyle = headerStyle;

        for (int i = 0; i < 3; i++)
        {
            sheet.AutoSizeColumn(i);
        }

        using var ms = new MemoryStream();
        workbook.Write(ms);
        return ms.ToArray();
    }
}


