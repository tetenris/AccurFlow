using AccuFlow.Application.Features.FinancialStatements.Queries;
using MediatR;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace AccuFlow.Application.Features.FinancialStatements.Handlers
{
    public class ExportBalanceSheetQueryHandler : IRequestHandler<ExportBalanceSheetQuery, byte[]>
    {
        private readonly ISender _mediator;

        public ExportBalanceSheetQueryHandler(ISender mediator)
        {
            _mediator = mediator;
        }

        public async Task<byte[]> Handle(ExportBalanceSheetQuery request, CancellationToken cancellationToken)
        {
            var balanceSheet = await _mediator.Send(new GetBalanceSheetQuery(request.Request), cancellationToken);

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
    }
}