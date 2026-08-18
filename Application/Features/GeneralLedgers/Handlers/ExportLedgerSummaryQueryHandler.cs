using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.GeneralLedgers.Queries;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.GeneralLedgers.Handlers
{
    public class ExportLedgerSummaryQueryHandler : IRequestHandler<ExportLedgerSummaryQuery, byte[]>
    {
        private readonly GetLedgerSummaryQueryHandler _summaryHandler;

        public ExportLedgerSummaryQueryHandler(IRepository<JournalLineEntity> journalLineRepository)
        {
            _summaryHandler = new GetLedgerSummaryQueryHandler(journalLineRepository);
        }

        public async Task<byte[]> Handle(ExportLedgerSummaryQuery request, CancellationToken cancellationToken)
        {
            var summary = await _summaryHandler.Handle(new GetLedgerSummaryQuery(request.Request), cancellationToken);

            using (var workbook = new NPOI.XSSF.UserModel.XSSFWorkbook())
            {
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

                if (request.Request.DateFrom.HasValue || request.Request.DateTo.HasValue)
                {
                    var periodRow = sheet.CreateRow(rowIndex++);
                    var periodCell = periodRow.CreateCell(0);
                    var period = "";
                    if (request.Request.DateFrom.HasValue && request.Request.DateTo.HasValue)
                        period = $"Period: {request.Request.DateFrom.Value:dd/MM/yyyy} - {request.Request.DateTo.Value:dd/MM/yyyy}";
                    else if (request.Request.DateFrom.HasValue)
                        period = $"From: {request.Request.DateFrom.Value:dd/MM/yyyy}";
                    else if (request.Request.DateTo.HasValue)
                        period = $"To: {request.Request.DateTo.Value:dd/MM/yyyy}";
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
        }
    }
}