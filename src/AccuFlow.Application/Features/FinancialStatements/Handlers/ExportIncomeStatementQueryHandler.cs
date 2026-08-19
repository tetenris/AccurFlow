using AccuFlow.Application.Features.FinancialStatements.Queries;
using MediatR;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace AccuFlow.Application.Features.FinancialStatements.Handlers
{
    public class ExportIncomeStatementQueryHandler : IRequestHandler<ExportIncomeStatementQuery, byte[]>
    {
        private readonly ISender _mediator;

        public ExportIncomeStatementQueryHandler(ISender mediator)
        {
            _mediator = mediator;
        }

        public async Task<byte[]> Handle(ExportIncomeStatementQuery request, CancellationToken cancellationToken)
        {
            var incomeStatement = await _mediator.Send(new GetIncomeStatementQuery(request.Request), cancellationToken);

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
    }
}