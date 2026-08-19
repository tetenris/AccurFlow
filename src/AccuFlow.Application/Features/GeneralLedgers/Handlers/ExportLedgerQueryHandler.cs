using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.GeneralLedgers.Queries;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.GeneralLedgers.Handlers
{
    public class ExportLedgerQueryHandler : IRequestHandler<ExportLedgerQuery, byte[]>
    {
        private readonly GetLedgerQueryHandler _ledgerHandler;

        public ExportLedgerQueryHandler(
            IRepository<JournalLineEntity> journalLineRepository,
            IRepository<ChartOfAccountEntity> coaRepository)
        {
            _ledgerHandler = new GetLedgerQueryHandler(journalLineRepository, coaRepository);
        }

        public async Task<byte[]> Handle(ExportLedgerQuery request, CancellationToken cancellationToken)
        {
            var ledger = await _ledgerHandler.Handle(new GetLedgerQuery(request.Request), cancellationToken);

            using (var workbook = new NPOI.XSSF.UserModel.XSSFWorkbook())
            {
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
        }
    }
}