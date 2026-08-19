using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Suppliers.Queries;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Suppliers.Handlers
{
    public class ExportSupplierQueryHandler : IRequestHandler<ExportSupplierQuery, byte[]>
    {
        private readonly IRepository<SupplierEntity> _supplierRepository;

        public ExportSupplierQueryHandler(IRepository<SupplierEntity> supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<byte[]> Handle(ExportSupplierQuery request, CancellationToken cancellationToken)
        {
            IQueryable<SupplierEntity> query = _supplierRepository.Query()
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrEmpty(request.SupplierType))
            {
                query = query.Where(x => x.SupplierType == request.SupplierType);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            var suppliers = await query
                .OrderBy(x => x.SupplierCode)
                .ToListAsync(cancellationToken);

            using (var workbook = new NPOI.XSSF.UserModel.XSSFWorkbook())
            {
                var sheet = workbook.CreateSheet("Suppliers");

                var headerStyle = workbook.CreateCellStyle();
                var headerFont = workbook.CreateFont();
                headerFont.IsBold = true;
                headerStyle.SetFont(headerFont);

                var headerRow = sheet.CreateRow(0);
                var headers = new[] {
                    "Supplier Code", "Supplier Name", "Type", "Contact Person",
                    "Phone", "Email", "Address", "City", "State", "Postal Code",
                    "Country", "Credit Limit", "Payment Terms", "Current Balance",
                    "Tax ID", "Status"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = headerRow.CreateCell(i);
                    cell.SetCellValue(headers[i]);
                    cell.CellStyle = headerStyle;
                }

                int rowIndex = 1;
                foreach (var supplier in suppliers)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(supplier.SupplierCode);
                    row.CreateCell(1).SetCellValue(supplier.SupplierName);
                    row.CreateCell(2).SetCellValue(supplier.SupplierType);
                    row.CreateCell(3).SetCellValue(supplier.ContactPerson ?? "");
                    row.CreateCell(4).SetCellValue(supplier.Phone ?? "");
                    row.CreateCell(5).SetCellValue(supplier.Email ?? "");
                    row.CreateCell(6).SetCellValue(supplier.Address ?? "");
                    row.CreateCell(7).SetCellValue(supplier.City ?? "");
                    row.CreateCell(8).SetCellValue(supplier.State ?? "");
                    row.CreateCell(9).SetCellValue(supplier.PostalCode ?? "");
                    row.CreateCell(10).SetCellValue(supplier.Country ?? "");
                    row.CreateCell(11).SetCellValue((double)supplier.CreditLimit);
                    row.CreateCell(12).SetCellValue(supplier.PaymentTerms);
                    row.CreateCell(13).SetCellValue((double)supplier.CurrentBalance);
                    row.CreateCell(14).SetCellValue(supplier.TaxId ?? "");
                    row.CreateCell(15).SetCellValue(supplier.IsActive ? "Active" : "Inactive");
                }

                for (int i = 0; i < headers.Length; i++)
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