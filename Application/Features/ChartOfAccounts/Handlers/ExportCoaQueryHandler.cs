using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ChartOfAccounts.Queries;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ChartOfAccounts.Handlers
{
    public class ExportCoaQueryHandler : IRequestHandler<ExportCoaQuery, byte[]>
    {
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;

        public ExportCoaQueryHandler(IRepository<ChartOfAccountEntity> coaRepository)
        {
            _coaRepository = coaRepository;
        }

        public async Task<byte[]> Handle(ExportCoaQuery request, CancellationToken cancellationToken)
        {
            IQueryable<ChartOfAccountEntity> query = _coaRepository.Query()
                .Where(x => !x.IsDeleted)
                .Include(x => x.ParentAccount)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.AccountType))
            {
                query = query.Where(x => x.AccountType == request.AccountType);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
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
                .ToListAsync(cancellationToken);

            using (var workbook = new NPOI.XSSF.UserModel.XSSFWorkbook())
            {
                var sheet = workbook.CreateSheet("Chart of Accounts");

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

                var headerRow = sheet.CreateRow(0);
                string[] headers = { "Account Code", "Account Name", "Type", "Parent Account", "Description", "Normal Balance", "Opening Balance", "Currency", "Is Header", "Status", "Level" };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = headerRow.CreateCell(i);
                    cell.SetCellValue(headers[i]);
                    cell.CellStyle = headerStyle;
                }

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

                for (int i = 0; i < headers.Length; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                using (var ms = new MemoryStream())
                {
                    workbook.Write(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}