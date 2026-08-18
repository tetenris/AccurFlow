using AccuFlow.Application.Features.ChartOfAccounts.Queries;
using MediatR;

namespace AccuFlow.Application.Features.ChartOfAccounts.Handlers
{
    public class DownloadCoaTemplateQueryHandler : IRequestHandler<DownloadCoaTemplateQuery, byte[]>
    {
        private static readonly string[] TemplateHeaders =
        {
            "Account Code",
            "Account Name",
            "Account Type",
            "Parent Account Code",
            "Description",
            "Is Header",
            "Is Active",
            "Opening Balance",
            "Currency"
        };

        public Task<byte[]> Handle(DownloadCoaTemplateQuery request, CancellationToken cancellationToken)
        {
            using (var workbook = new NPOI.XSSF.UserModel.XSSFWorkbook())
            {
                var sheet = workbook.CreateSheet("ChartOfAccounts");

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

                var dataStyle = workbook.CreateCellStyle();
                dataStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                dataStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                dataStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                dataStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;

                var headerRow = sheet.CreateRow(0);
                for (int i = 0; i < TemplateHeaders.Length; i++)
                {
                    var cell = headerRow.CreateCell(i);
                    cell.SetCellValue(TemplateHeaders[i]);
                    cell.CellStyle = headerStyle;
                }

                var examples = new object[][]
                {
                    new object[] { "1-20000", "Non-Current Assets", "Asset", "", "EXAMPLE - header/root account. Delete this row before importing real data.", "Yes", "Yes", 0, "IDR" },
                    new object[] { "1-20100", "Fixed Assets", "Asset", "1-20000", "EXAMPLE - sub-account of Non-Current Assets.", "No", "Yes", 0, "IDR" },
                    new object[] { "1-20200", "Accumulated Depreciation", "Asset", "1-20000", "EXAMPLE - sub-account of Non-Current Assets.", "No", "Yes", 0, "IDR" }
                };

                for (int r = 0; r < examples.Length; r++)
                {
                    var row = sheet.CreateRow(r + 1);
                    for (int c = 0; c < examples[r].Length; c++)
                    {
                        var cell = row.CreateCell(c);
                        cell.SetCellValue(examples[r][c].ToString() ?? "");
                        cell.CellStyle = dataStyle;
                    }
                }

                var instructionSheet = workbook.CreateSheet("Instructions");
                var instructionHeaderRow = instructionSheet.CreateRow(0);
                instructionHeaderRow.CreateCell(0).SetCellValue("Column");
                instructionHeaderRow.CreateCell(1).SetCellValue("Required");
                instructionHeaderRow.CreateCell(2).SetCellValue("Description");
                for (int i = 0; i < 3; i++)
                {
                    instructionHeaderRow.GetCell(i).CellStyle = headerStyle;
                }

                string[,] instructions =
                {
                    { "Account Code", "Yes", "Unique account code. Recommended format: X-XXXXX (e.g., 1-10000). First digit follows the Account Type." },
                    { "Account Name", "Yes", "Account name (3-255 characters)." },
                    { "Account Type", "Yes", "One of: Asset, Liability, Equity, Revenue, Expense, Other Income, Other Expense." },
                    { "Parent Account Code", "No", "Code of the parent account (must already exist or be listed earlier in the file). Leave blank for root account." },
                    { "Description", "No", "Optional description (max 500 characters)." },
                    { "Is Header", "Yes", "Yes/No. Header accounts cannot post transactions." },
                    { "Is Active", "Yes", "Yes/No." },
                    { "Opening Balance", "No", "Numeric opening balance. Default 0." },
                    { "Currency", "No", "Currency code, e.g., IDR. Default IDR." }
                };

                for (int r = 0; r < instructions.GetLength(0); r++)
                {
                    var row = instructionSheet.CreateRow(r + 1);
                    for (int c = 0; c < 3; c++)
                    {
                        var cell = row.CreateCell(c);
                        cell.SetCellValue(instructions[r, c]);
                        cell.CellStyle = dataStyle;
                    }
                }

                for (int i = 0; i < TemplateHeaders.Length; i++)
                {
                    sheet.AutoSizeColumn(i);
                }
                for (int i = 0; i < 3; i++)
                {
                    instructionSheet.AutoSizeColumn(i);
                }

                using (var ms = new MemoryStream())
                {
                    workbook.Write(ms);
                    return Task.FromResult(ms.ToArray());
                }
            }
        }
    }
}