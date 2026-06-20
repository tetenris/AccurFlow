using AccuFlow.Entities;
using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using AccuFlow.Services;
using AccuFlow.Models.GeneralLedger;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace AccuFlow.Services;

public class GeneralLedgerService : BaseService, IGeneralLedgerService
{
    public GeneralLedgerService(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<decimal> GetAccountBalanceAsync(Guid accountId, DateTime? asOfDate = null)
    {
        var account = await _dbContext.Set<ChartOfAccountEntity>()
            .FirstOrDefaultAsync(a => a.AccountId == accountId);

        if (account == null)
            return 0;

        var query = _dbContext.Set<JournalLineEntity>()
            .Include(jl => jl.JournalEntry)
            .Where(jl => jl.AccountId == accountId
                && jl.JournalEntry!.Status == "Posted"
                && !jl.IsDeleted
                && !jl.JournalEntry.IsDeleted);

        if (asOfDate.HasValue)
        {
            query = query.Where(jl => jl.JournalEntry!.JournalDate <= asOfDate.Value);
        }

        var totalDebit = await query.SumAsync(jl => jl.DebitAmount);
        var totalCredit = await query.SumAsync(jl => jl.CreditAmount);

        return CalculateBalance(account.AccountType, totalDebit, totalCredit);
    }

    public async Task<decimal> GetOpeningBalanceAsync(Guid accountId, DateTime dateFrom)
    {
        var account = await _dbContext.Set<ChartOfAccountEntity>()
            .FirstOrDefaultAsync(a => a.AccountId == accountId);

        if (account == null)
            return 0;

        var totalDebit = await _dbContext.Set<JournalLineEntity>()
            .Include(jl => jl.JournalEntry)
            .Where(jl => jl.AccountId == accountId
                && jl.JournalEntry!.Status == "Posted"
                && jl.JournalEntry.JournalDate < dateFrom
                && !jl.IsDeleted
                && !jl.JournalEntry.IsDeleted)
            .SumAsync(jl => jl.DebitAmount);

        var totalCredit = await _dbContext.Set<JournalLineEntity>()
            .Include(jl => jl.JournalEntry)
            .Where(jl => jl.AccountId == accountId
                && jl.JournalEntry!.Status == "Posted"
                && jl.JournalEntry.JournalDate < dateFrom
                && !jl.IsDeleted
                && !jl.JournalEntry.IsDeleted)
            .SumAsync(jl => jl.CreditAmount);

        return CalculateBalance(account.AccountType, totalDebit, totalCredit);
    }

    public async Task<AccountLedgerViewModel> GetAccountLedgerAsync(GetLedgerRequest request)
    {
        var account = await _dbContext.Set<ChartOfAccountEntity>()
            .FirstOrDefaultAsync(a => a.AccountId == request.AccountId);

        if (account == null)
            throw new Exception("Account not found");

        var query = _dbContext.Set<JournalLineEntity>()
            .Include(jl => jl.JournalEntry)
            .ThenInclude(je => je!.PostedByUser)
            .Where(jl => jl.AccountId == request.AccountId
                && (jl.JournalEntry!.Status == "Posted" || jl.JournalEntry.Status == "Reversed")
                && !jl.IsDeleted
                && !jl.JournalEntry.IsDeleted);

        if (request.DateFrom.HasValue)
        {
            query = query.Where(jl => jl.JournalEntry!.JournalDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(jl => jl.JournalEntry!.JournalDate <= request.DateTo.Value);
        }

        var entries = await query
            .OrderBy(jl => jl.JournalEntry!.JournalDate)
            .ThenBy(jl => jl.JournalEntry!.JournalNumber)
            .Select(jl => new LedgerEntryViewModel
            {
                JournalLineId = jl.JournalLineId,
                JournalId = jl.JournalId,
                JournalNumber = jl.JournalEntry!.JournalNumber,
                JournalDate = jl.JournalEntry.JournalDate,
                Description = jl.Description,
                DebitAmount = jl.DebitAmount,
                CreditAmount = jl.CreditAmount,
                PostedBy = jl.JournalEntry.PostedByUser != null ? jl.JournalEntry.PostedByUser.FullName : "",
                PostedDate = jl.JournalEntry.PostedDate ?? DateTime.Now,
                Status = jl.JournalEntry.Status,
                IsReversal = jl.JournalEntry.OriginalJournalId != null
            })
            .ToListAsync();

        decimal openingBalance = 0;
        if (request.DateFrom.HasValue)
        {
            openingBalance = await GetOpeningBalanceAsync(request.AccountId, request.DateFrom.Value);
        }

        decimal runningBalance = openingBalance;
        foreach (var entry in entries)
        {
            if (account.AccountType is "Asset" or "Expense" or "Other Expense")
            {
                runningBalance += entry.DebitAmount - entry.CreditAmount;
            }
            else
            {
                runningBalance += entry.CreditAmount - entry.DebitAmount;
            }
            entry.RunningBalance = runningBalance;
        }

        var totalDebit = entries.Sum(e => e.DebitAmount);
        var totalCredit = entries.Sum(e => e.CreditAmount);

        return new AccountLedgerViewModel
        {
            AccountId = account.AccountId,
            AccountCode = account.AccountCode,
            AccountName = account.AccountName,
            AccountType = account.AccountType,
            OpeningBalance = openingBalance,
            ClosingBalance = runningBalance,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            Entries = entries
        };
    }

    public async Task<List<LedgerSummaryViewModel>> GetLedgerSummaryAsync(GetLedgerSummaryRequest request)
    {
        var query = _dbContext.Set<JournalLineEntity>()
            .Include(jl => jl.JournalEntry)
            .Include(jl => jl.Account)
            .Where(jl => (jl.JournalEntry!.Status == "Posted" || jl.JournalEntry.Status == "Reversed")
                && !jl.IsDeleted
                && !jl.JournalEntry.IsDeleted);

        if (request.DateFrom.HasValue)
        {
            query = query.Where(jl => jl.JournalEntry!.JournalDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(jl => jl.JournalEntry!.JournalDate <= request.DateTo.Value);
        }

        if (!string.IsNullOrEmpty(request.AccountType))
        {
            query = query.Where(jl => jl.Account!.AccountType == request.AccountType);
        }

        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(jl => jl.Account!.AccountCode.Contains(request.Search)
                || jl.Account!.AccountName.Contains(request.Search));
        }

        var grouped = await query
            .GroupBy(jl => new
            {
                jl.AccountId,
                jl.Account!.AccountCode,
                jl.Account.AccountName,
                jl.Account.AccountType
            })
            .Select(g => new
            {
                g.Key.AccountId,
                g.Key.AccountCode,
                g.Key.AccountName,
                g.Key.AccountType,
                TotalDebit = g.Sum(jl => jl.DebitAmount),
                TotalCredit = g.Sum(jl => jl.CreditAmount),
                TransactionCount = g.Count()
            })
            .ToListAsync();

        return grouped.Select(g => new LedgerSummaryViewModel
        {
            AccountId = g.AccountId,
            AccountCode = g.AccountCode,
            AccountName = g.AccountName,
            AccountType = g.AccountType,
            TotalDebit = g.TotalDebit,
            TotalCredit = g.TotalCredit,
            Balance = CalculateBalance(g.AccountType, g.TotalDebit, g.TotalCredit),
            TransactionCount = g.TransactionCount
        }).ToList();
    }

    public async Task<byte[]> ExportLedgerToExcelAsync(GetLedgerRequest request)
    {
        var ledger = await GetAccountLedgerAsync(request);

        var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("Ledger");

        var headerStyle = workbook.CreateCellStyle();
        var headerFont = workbook.CreateFont();
        headerFont.IsBold = true;
        headerStyle.SetFont(headerFont);

        int rowIndex = 0;

        var titleRow = sheet.CreateRow(rowIndex++);
        var titleCell = titleRow.CreateCell(0);
        titleCell.SetCellValue($"General Ledger - {ledger.AccountCode} {ledger.AccountName}");
        titleCell.CellStyle = headerStyle;

        if (request.DateFrom.HasValue || request.DateTo.HasValue)
        {
            var periodRow = sheet.CreateRow(rowIndex++);
            var periodCell = periodRow.CreateCell(0);
            var period = "";
            if (request.DateFrom.HasValue && request.DateTo.HasValue)
                period = $"Period: {request.DateFrom.Value:dd/MM/yyyy} - {request.DateTo.Value:dd/MM/yyyy}";
            else if (request.DateFrom.HasValue)
                period = $"From: {request.DateFrom.Value:dd/MM/yyyy}";
            else if (request.DateTo.HasValue)
                period = $"To: {request.DateTo.Value:dd/MM/yyyy}";
            periodCell.SetCellValue(period);
        }

        rowIndex++;

        var openingRow = sheet.CreateRow(rowIndex++);
        openingRow.CreateCell(0).SetCellValue("Opening Balance");
        openingRow.CreateCell(5).SetCellValue((double)ledger.OpeningBalance);

        var headerRow = sheet.CreateRow(rowIndex++);
        headerRow.CreateCell(0).SetCellValue("Date");
        headerRow.CreateCell(1).SetCellValue("Journal Number");
        headerRow.CreateCell(2).SetCellValue("Description");
        headerRow.CreateCell(3).SetCellValue("Debit");
        headerRow.CreateCell(4).SetCellValue("Credit");
        headerRow.CreateCell(5).SetCellValue("Balance");
        for (int i = 0; i < 6; i++)
        {
            headerRow.GetCell(i).CellStyle = headerStyle;
        }

        foreach (var entry in ledger.Entries)
        {
            var row = sheet.CreateRow(rowIndex++);
            row.CreateCell(0).SetCellValue(entry.JournalDate.ToString("dd/MM/yyyy"));
            row.CreateCell(1).SetCellValue(entry.JournalNumber);
            row.CreateCell(2).SetCellValue(entry.Description);
            row.CreateCell(3).SetCellValue((double)entry.DebitAmount);
            row.CreateCell(4).SetCellValue((double)entry.CreditAmount);
            row.CreateCell(5).SetCellValue((double)entry.RunningBalance);
        }

        rowIndex++;
        var totalRow = sheet.CreateRow(rowIndex++);
        totalRow.CreateCell(2).SetCellValue("Total");
        totalRow.CreateCell(3).SetCellValue((double)ledger.TotalDebit);
        totalRow.CreateCell(4).SetCellValue((double)ledger.TotalCredit);
        for (int i = 2; i < 5; i++)
        {
            totalRow.GetCell(i).CellStyle = headerStyle;
        }

        var closingRow = sheet.CreateRow(rowIndex++);
        closingRow.CreateCell(0).SetCellValue("Closing Balance");
        closingRow.CreateCell(5).SetCellValue((double)ledger.ClosingBalance);
        closingRow.GetCell(0).CellStyle = headerStyle;
        closingRow.GetCell(5).CellStyle = headerStyle;

        for (int i = 0; i < 6; i++)
        {
            sheet.AutoSizeColumn(i);
        }

        using var ms = new MemoryStream();
        workbook.Write(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportSummaryToExcelAsync(GetLedgerSummaryRequest request)
    {
        var summary = await GetLedgerSummaryAsync(request);

        var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("Ledger Summary");

        var headerStyle = workbook.CreateCellStyle();
        var headerFont = workbook.CreateFont();
        headerFont.IsBold = true;
        headerStyle.SetFont(headerFont);

        int rowIndex = 0;

        var titleRow = sheet.CreateRow(rowIndex++);
        var titleCell = titleRow.CreateCell(0);
        titleCell.SetCellValue("General Ledger Summary");
        titleCell.CellStyle = headerStyle;

        if (request.DateFrom.HasValue || request.DateTo.HasValue)
        {
            var periodRow = sheet.CreateRow(rowIndex++);
            var periodCell = periodRow.CreateCell(0);
            var period = "";
            if (request.DateFrom.HasValue && request.DateTo.HasValue)
                period = $"Period: {request.DateFrom.Value:dd/MM/yyyy} - {request.DateTo.Value:dd/MM/yyyy}";
            else if (request.DateFrom.HasValue)
                period = $"From: {request.DateFrom.Value:dd/MM/yyyy}";
            else if (request.DateTo.HasValue)
                period = $"To: {request.DateTo.Value:dd/MM/yyyy}";
            periodCell.SetCellValue(period);
        }

        rowIndex++;

        var headerRow = sheet.CreateRow(rowIndex++);
        headerRow.CreateCell(0).SetCellValue("Account Code");
        headerRow.CreateCell(1).SetCellValue("Account Name");
        headerRow.CreateCell(2).SetCellValue("Type");
        headerRow.CreateCell(3).SetCellValue("Total Debit");
        headerRow.CreateCell(4).SetCellValue("Total Credit");
        headerRow.CreateCell(5).SetCellValue("Balance");
        headerRow.CreateCell(6).SetCellValue("Transactions");
        for (int i = 0; i < 7; i++)
        {
            headerRow.GetCell(i).CellStyle = headerStyle;
        }

        foreach (var item in summary)
        {
            var row = sheet.CreateRow(rowIndex++);
            row.CreateCell(0).SetCellValue(item.AccountCode);
            row.CreateCell(1).SetCellValue(item.AccountName);
            row.CreateCell(2).SetCellValue(item.AccountType);
            row.CreateCell(3).SetCellValue((double)item.TotalDebit);
            row.CreateCell(4).SetCellValue((double)item.TotalCredit);
            row.CreateCell(5).SetCellValue((double)item.Balance);
            row.CreateCell(6).SetCellValue(item.TransactionCount);
        }

        rowIndex++;
        var totalRow = sheet.CreateRow(rowIndex++);
        totalRow.CreateCell(2).SetCellValue("Total");
        totalRow.CreateCell(3).SetCellValue((double)summary.Sum(s => s.TotalDebit));
        totalRow.CreateCell(4).SetCellValue((double)summary.Sum(s => s.TotalCredit));
        for (int i = 2; i < 5; i++)
        {
            totalRow.GetCell(i).CellStyle = headerStyle;
        }

        for (int i = 0; i < 7; i++)
        {
            sheet.AutoSizeColumn(i);
        }

        using var ms = new MemoryStream();
        workbook.Write(ms);
        return ms.ToArray();
    }

    private decimal CalculateBalance(string accountType, decimal totalDebit, decimal totalCredit)
    {
        return accountType switch
        {
            "Asset" => totalDebit - totalCredit,
            "Expense" => totalDebit - totalCredit,
            "Other Expense" => totalDebit - totalCredit,
            "Liability" => totalCredit - totalDebit,
            "Equity" => totalCredit - totalDebit,
            "Revenue" => totalCredit - totalDebit,
            "Other Income" => totalCredit - totalDebit,
            _ => 0
        };
    }
}
