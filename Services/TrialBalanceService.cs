using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using AccuFlow.Services;
using KomatsuERP.Models.TrialBalance;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace KomatsuERP.Services;

public class TrialBalanceService : BaseService, ITrialBalanceService
{
    private readonly IGeneralLedgerService _generalLedgerService;

    public TrialBalanceService(AppDbContext dbContext, IGeneralLedgerService generalLedgerService) : base(dbContext)
    {
        _generalLedgerService = generalLedgerService;
    }

    public async Task<TrialBalanceViewModel> GetTrialBalanceAsync(GetTrialBalanceRequest request)
    {
        var asOfDate = request.AsOfDate ?? DateTime.Now;

        var accountsQuery = _dbContext.Set<ChartOfAccountEntity>()
            .Where(a => !a.IsDeleted && a.IsActive && !a.IsHeader);

        if (!string.IsNullOrEmpty(request.AccountType))
        {
            accountsQuery = accountsQuery.Where(a => a.AccountType == request.AccountType);
        }

        var accounts = await accountsQuery.OrderBy(a => a.AccountCode).ToListAsync();

        var lines = new List<TrialBalanceLineViewModel>();

        foreach (var account in accounts)
        {
            var balance = await _generalLedgerService.GetAccountBalanceAsync(account.AccountId, asOfDate);

            if (!request.ShowZeroBalance && balance == 0)
                continue;

            var line = new TrialBalanceLineViewModel
            {
                AccountId = account.AccountId,
                AccountCode = account.AccountCode,
                AccountName = account.AccountName,
                AccountType = account.AccountType
            };

            if (account.AccountType is "Asset" or "Expense" or "Other Expense")
            {
                line.DebitBalance = balance >= 0 ? balance : 0;
                line.CreditBalance = balance < 0 ? Math.Abs(balance) : 0;
            }
            else
            {
                line.CreditBalance = balance >= 0 ? balance : 0;
                line.DebitBalance = balance < 0 ? Math.Abs(balance) : 0;
            }

            lines.Add(line);
        }

        var groups = lines
            .GroupBy(l => l.AccountType)
            .OrderBy(g => GetAccountTypeOrder(g.Key))
            .Select(g => new TrialBalanceGroupViewModel
            {
                AccountType = g.Key,
                Accounts = g.OrderBy(a => a.AccountCode).ToList(),
                SubtotalDebit = g.Sum(a => a.DebitBalance),
                SubtotalCredit = g.Sum(a => a.CreditBalance)
            })
            .ToList();

        var totalDebit = groups.Sum(g => g.SubtotalDebit);
        var totalCredit = groups.Sum(g => g.SubtotalCredit);
        var difference = totalDebit - totalCredit;

        return new TrialBalanceViewModel
        {
            AsOfDate = asOfDate,
            Groups = groups,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            Difference = difference,
            IsBalanced = Math.Abs(difference) < 0.01m
        };
    }

    public async Task<byte[]> ExportToExcelAsync(GetTrialBalanceRequest request)
    {
        var trialBalance = await GetTrialBalanceAsync(request);

        var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("Trial Balance");

        var headerStyle = workbook.CreateCellStyle();
        var headerFont = workbook.CreateFont();
        headerFont.IsBold = true;
        headerStyle.SetFont(headerFont);

        int rowIndex = 0;

        var titleRow = sheet.CreateRow(rowIndex++);
        var titleCell = titleRow.CreateCell(0);
        titleCell.SetCellValue("Trial Balance");
        titleCell.CellStyle = headerStyle;

        var dateRow = sheet.CreateRow(rowIndex++);
        dateRow.CreateCell(0).SetCellValue($"As of: {trialBalance.AsOfDate:dd/MM/yyyy}");

        rowIndex++;

        var headerRow = sheet.CreateRow(rowIndex++);
        headerRow.CreateCell(0).SetCellValue("Account Code");
        headerRow.CreateCell(1).SetCellValue("Account Name");
        headerRow.CreateCell(2).SetCellValue("Debit");
        headerRow.CreateCell(3).SetCellValue("Credit");
        for (int i = 0; i < 4; i++)
        {
            headerRow.GetCell(i).CellStyle = headerStyle;
        }

        foreach (var group in trialBalance.Groups)
        {
            var groupRow = sheet.CreateRow(rowIndex++);
            groupRow.CreateCell(0).SetCellValue($"Account Type: {group.AccountType}");
            groupRow.GetCell(0).CellStyle = headerStyle;

            foreach (var account in group.Accounts)
            {
                var row = sheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(account.AccountCode);
                row.CreateCell(1).SetCellValue(account.AccountName);
                row.CreateCell(2).SetCellValue((double)account.DebitBalance);
                row.CreateCell(3).SetCellValue((double)account.CreditBalance);
            }

            var subtotalRow = sheet.CreateRow(rowIndex++);
            subtotalRow.CreateCell(1).SetCellValue("Subtotal");
            subtotalRow.CreateCell(2).SetCellValue((double)group.SubtotalDebit);
            subtotalRow.CreateCell(3).SetCellValue((double)group.SubtotalCredit);
            for (int i = 1; i < 4; i++)
            {
                subtotalRow.GetCell(i).CellStyle = headerStyle;
            }

            rowIndex++;
        }

        var totalRow = sheet.CreateRow(rowIndex++);
        totalRow.CreateCell(1).SetCellValue("TOTAL");
        totalRow.CreateCell(2).SetCellValue((double)trialBalance.TotalDebit);
        totalRow.CreateCell(3).SetCellValue((double)trialBalance.TotalCredit);
        for (int i = 1; i < 4; i++)
        {
            totalRow.GetCell(i).CellStyle = headerStyle;
        }

        if (!trialBalance.IsBalanced)
        {
            var diffRow = sheet.CreateRow(rowIndex++);
            diffRow.CreateCell(1).SetCellValue("Difference");
            diffRow.CreateCell(2).SetCellValue((double)Math.Abs(trialBalance.Difference));
        }

        for (int i = 0; i < 4; i++)
        {
            sheet.AutoSizeColumn(i);
        }

        using var ms = new MemoryStream();
        workbook.Write(ms);
        return ms.ToArray();
    }

    private int GetAccountTypeOrder(string accountType)
    {
        return accountType switch
        {
            "Asset" => 1,
            "Liability" => 2,
            "Equity" => 3,
            "Revenue" => 4,
            "Expense" => 5,
            "Other Income" => 6,
            "Other Expense" => 7,
            _ => 99
        };
    }
}
