using AccuFlow.Application.Features.FinancialStatements.Queries;
using MediatR;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace AccuFlow.Application.Features.FinancialStatements.Handlers
{
    public class ExportCashFlowQueryHandler : IRequestHandler<ExportCashFlowQuery, byte[]>
    {
        private readonly ISender _mediator;

        public ExportCashFlowQueryHandler(ISender mediator)
        {
            _mediator = mediator;
        }

        public async Task<byte[]> Handle(ExportCashFlowQuery request, CancellationToken cancellationToken)
        {
            var cashFlow = await _mediator.Send(new GetCashFlowQuery(request.Request), cancellationToken);

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
}