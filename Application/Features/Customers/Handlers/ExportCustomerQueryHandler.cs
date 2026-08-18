using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Customers.Queries;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Customers.Handlers
{
    public class ExportCustomerQueryHandler : IRequestHandler<ExportCustomerQuery, byte[]>
    {
        private readonly IRepository<CustomerEntity> _customerRepository;

        public ExportCustomerQueryHandler(IRepository<CustomerEntity> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<byte[]> Handle(ExportCustomerQuery request, CancellationToken cancellationToken)
        {
            IQueryable<CustomerEntity> query = _customerRepository.Query()
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrEmpty(request.CustomerType))
            {
                query = query.Where(x => x.CustomerType == request.CustomerType);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            var customers = await query
                .OrderBy(x => x.CustomerCode)
                .ToListAsync(cancellationToken);

            using (var workbook = new NPOI.XSSF.UserModel.XSSFWorkbook())
            {
                var sheet = workbook.CreateSheet("Customers");

                var headerStyle = workbook.CreateCellStyle();
                var headerFont = workbook.CreateFont();
                headerFont.IsBold = true;
                headerStyle.SetFont(headerFont);

                var headerRow = sheet.CreateRow(0);
                var headers = new[] {
                    "Customer Code", "Customer Name", "Type", "Contact Person",
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
                foreach (var customer in customers)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(customer.CustomerCode);
                    row.CreateCell(1).SetCellValue(customer.CustomerName);
                    row.CreateCell(2).SetCellValue(customer.CustomerType);
                    row.CreateCell(3).SetCellValue(customer.ContactPerson ?? "");
                    row.CreateCell(4).SetCellValue(customer.Phone ?? "");
                    row.CreateCell(5).SetCellValue(customer.Email ?? "");
                    row.CreateCell(6).SetCellValue(customer.Address ?? "");
                    row.CreateCell(7).SetCellValue(customer.City ?? "");
                    row.CreateCell(8).SetCellValue(customer.State ?? "");
                    row.CreateCell(9).SetCellValue(customer.PostalCode ?? "");
                    row.CreateCell(10).SetCellValue(customer.Country ?? "");
                    row.CreateCell(11).SetCellValue((double)customer.CreditLimit);
                    row.CreateCell(12).SetCellValue(customer.PaymentTerms);
                    row.CreateCell(13).SetCellValue((double)customer.CurrentBalance);
                    row.CreateCell(14).SetCellValue(customer.TaxId ?? "");
                    row.CreateCell(15).SetCellValue(customer.IsActive ? "Active" : "Inactive");
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