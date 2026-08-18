using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.JournalEntries.Queries;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.JournalEntries.Handlers
{
    public class ExportJournalQueryHandler : IRequestHandler<ExportJournalQuery, byte[]>
    {
        private readonly IRepository<JournalEntryEntity> _journalRepository;

        public ExportJournalQueryHandler(IRepository<JournalEntryEntity> journalRepository)
        {
            _journalRepository = journalRepository;
        }

        public async Task<byte[]> Handle(ExportJournalQuery request, CancellationToken cancellationToken)
        {
            IQueryable<JournalEntryEntity> query = _journalRepository.Query()
                .Where(x => !x.IsDeleted)
                .Include(x => x.JournalLines.Where(l => !l.IsDeleted))
                    .ThenInclude(l => l.Account)
                .Include(x => x.CreatedByUser)
                .Include(x => x.PostedByUser);

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

            var data = await query.OrderByDescending(x => x.JournalDate).ToListAsync(cancellationToken);

            using (var workbook = new NPOI.XSSF.UserModel.XSSFWorkbook())
            {
                var headerSheet = workbook.CreateSheet("Journal Headers");

                var headerStyle = workbook.CreateCellStyle();
                var headerFont = workbook.CreateFont();
                headerFont.IsBold = true;
                headerFont.FontHeightInPoints = 11;
                headerStyle.SetFont(headerFont);
                headerStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                headerStyle.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;

                var headerRow = headerSheet.CreateRow(0);
                string[] headers = { "Journal Number", "Date", "Description", "Status", "Total Debit", "Total Credit", "Posted Date", "Posted By", "Created By", "Created At" };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = headerRow.CreateCell(i);
                    cell.SetCellValue(headers[i]);
                    cell.CellStyle = headerStyle;
                }

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

                for (int i = 0; i < headers.Length; i++)
                {
                    headerSheet.AutoSizeColumn(i);
                }

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

                for (int i = 0; i < linesHeaders.Length; i++)
                {
                    linesSheet.AutoSizeColumn(i);
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