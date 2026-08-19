using AccuFlow.Application.Features.TrialBalances.Queries;
using MediatR;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace AccuFlow.Application.Features.TrialBalances.Handlers
{
    public class ExportTrialBalanceQueryHandler : IRequestHandler<ExportTrialBalanceQuery, byte[]>
    {
        private readonly ISender _mediator;

        public ExportTrialBalanceQueryHandler(ISender mediator)
        {
            _mediator = mediator;
        }

        public async Task<byte[]> Handle(ExportTrialBalanceQuery request, CancellationToken cancellationToken)
        {
            var trialBalance = await _mediator.Send(new GetTrialBalanceQuery(request.Request), cancellationToken);

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
    }
}