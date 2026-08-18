using AccuFlow.Entities.Context;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Customer;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace AccuFlow.Services
{
    public interface ICustomerService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableCustomerRequest request);
        Task<CustomerViewModel?> GetById(Guid customerId);
        Task<CustomerViewModel?> GetByCode(string customerCode);
        Task Create(CreateCustomerRequest request, Guid userId);
        Task Edit(UpdateCustomerRequest request, Guid userId);
        Task Delete(Guid customerId, Guid userId);
        Task ToggleStatus(Guid customerId, Guid userId);
        Task<List<CustomerDropdownViewModel>> GetActiveCustomersAsync();
        Task<bool> IsCodeUniqueAsync(string customerCode, Guid? excludeId = null);
        Task<string> GenerateCustomerCodeAsync();
        Task<byte[]> ExportToExcelAsync(string? customerType, bool? isActive);
    }

    public class CustomerService : BaseService, ICustomerService
    {
        public CustomerService(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<BaseDatatableResponse> Datatable(DataTableCustomerRequest request)
        {
            var query = _dbContext.Set<CustomerEntity>()
                .Where(x => !x.IsDeleted)
                .AsQueryable();

            var totalRecord = await query.CountAsync();

            if (!string.IsNullOrEmpty(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x =>
                    x.CustomerCode.ToLower().Contains(search) ||
                    x.CustomerName.ToLower().Contains(search) ||
                    (x.ContactPerson != null && x.ContactPerson.ToLower().Contains(search)) ||
                    (x.Phone != null && x.Phone.ToLower().Contains(search)) ||
                    (x.Email != null && x.Email.ToLower().Contains(search))
                );
            }

            if (!string.IsNullOrEmpty(request.CustomerType))
            {
                query = query.Where(x => x.CustomerType == request.CustomerType);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            query = request?.OrderBy?.ToLower() switch
            {
                "customercode" => request?.OrderType?.ToLower() == "asc"
                    ? query.OrderBy(x => x.CustomerCode)
                    : query.OrderByDescending(x => x.CustomerCode),
                "customername" => request?.OrderType?.ToLower() == "asc"
                    ? query.OrderBy(x => x.CustomerName)
                    : query.OrderByDescending(x => x.CustomerName),
                "customertype" => request?.OrderType?.ToLower() == "asc"
                    ? query.OrderBy(x => x.CustomerType)
                    : query.OrderByDescending(x => x.CustomerType),
                "createdat" => request?.OrderType?.ToLower() == "asc"
                    ? query.OrderBy(x => x.CreatedAt)
                    : query.OrderByDescending(x => x.CreatedAt),
                _ => query.OrderBy(x => x.CustomerCode)
            };

            var totalFiltered = await query.CountAsync();

            var data = await query
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .Select(x => new CustomerViewModel
                {
                    CustomerId = x.CustomerId,
                    CustomerCode = x.CustomerCode,
                    CustomerName = x.CustomerName,
                    CustomerType = x.CustomerType,
                    ContactPerson = x.ContactPerson,
                    Phone = x.Phone,
                    Email = x.Email,
                    Website = x.Website,
                    Address = x.Address,
                    City = x.City,
                    State = x.State,
                    PostalCode = x.PostalCode,
                    Country = x.Country,
                    CreditLimit = x.CreditLimit,
                    PaymentTerms = x.PaymentTerms,
                    CurrentBalance = x.CurrentBalance,
                    TaxId = x.TaxId,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    CreatedBy = x.CreatedBy,
                    CreatedAt = x.CreatedAt,
                    UpdatedBy = x.UpdatedBy,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            // Collect all unique user IDs
            var userIds = data
                .SelectMany(x => new[] { x.CreatedBy, x.UpdatedBy })
                .Where(id => !string.IsNullOrEmpty(id) && Guid.TryParse(id, out _))
                .Distinct()
                .Select(id => Guid.Parse(id))
                .ToList();

            // Load all users in one query
            var users = await _dbContext.Set<UserEntity>()
                .Where(u => userIds.Contains(u.UserId))
                .Select(u => new { u.UserId, Name = u.FullName ?? u.UserName })
                .ToDictionaryAsync(u => u.UserId.ToString(), u => u.Name);

            // Resolve user names
            foreach (var item in data)
            {
                if (!string.IsNullOrEmpty(item.CreatedBy) && users.TryGetValue(item.CreatedBy, out var createdByName))
                {
                    item.CreatedBy = createdByName;
                }
                else
                {
                    item.CreatedBy = "System";
                }

                if (!string.IsNullOrEmpty(item.UpdatedBy) && users.TryGetValue(item.UpdatedBy, out var updatedByName))
                {
                    item.UpdatedBy = updatedByName;
                }
            }

            return new BaseDatatableResponse
            {
                Draw = request.Draw,
                RecordsTotal = totalRecord,
                RecordsFiltered = totalFiltered,
                Data = data
            };
        }

        public async Task<CustomerViewModel?> GetById(Guid customerId)
        {
            var customer = await _dbContext.Set<CustomerEntity>()
                .Where(x => x.CustomerId == customerId && !x.IsDeleted)
                .Select(x => new CustomerViewModel
                {
                    CustomerId = x.CustomerId,
                    CustomerCode = x.CustomerCode,
                    CustomerName = x.CustomerName,
                    CustomerType = x.CustomerType,
                    ContactPerson = x.ContactPerson,
                    Phone = x.Phone,
                    Email = x.Email,
                    Website = x.Website,
                    Address = x.Address,
                    City = x.City,
                    State = x.State,
                    PostalCode = x.PostalCode,
                    Country = x.Country,
                    CreditLimit = x.CreditLimit,
                    PaymentTerms = x.PaymentTerms,
                    CurrentBalance = x.CurrentBalance,
                    TaxId = x.TaxId,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    CreatedBy = x.CreatedBy,
                    CreatedAt = x.CreatedAt,
                    UpdatedBy = x.UpdatedBy,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (customer != null)
            {
                // Resolve user names from user IDs
                customer.CreatedBy = await GetUserNameFromIdAsync(customer.CreatedBy ?? string.Empty);
                if (!string.IsNullOrEmpty(customer.UpdatedBy))
                {
                    customer.UpdatedBy = await GetUserNameFromIdAsync(customer.UpdatedBy);
                }
            }

            return customer;
        }

        public async Task<CustomerViewModel?> GetByCode(string customerCode)
        {
            return await _dbContext.Set<CustomerEntity>()
                .Where(x => x.CustomerCode == customerCode && !x.IsDeleted)
                .Select(x => new CustomerViewModel
                {
                    CustomerId = x.CustomerId,
                    CustomerCode = x.CustomerCode,
                    CustomerName = x.CustomerName,
                    CustomerType = x.CustomerType,
                    ContactPerson = x.ContactPerson,
                    Phone = x.Phone,
                    Email = x.Email,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync();
        }

        public async Task Create(CreateCustomerRequest request, Guid userId)
        {
            // Check if customer code already exists
            var existingCustomer = await _dbContext.Set<CustomerEntity>()
                .FirstOrDefaultAsync(x => x.CustomerCode == request.CustomerCode && !x.IsDeleted);

            if (existingCustomer != null)
            {
                throw new Exception($"Customer code '{request.CustomerCode}' already exists");
            }

            var customer = new CustomerEntity
            {
                CustomerId = Guid.NewGuid(),
                CustomerCode = request.CustomerCode,
                CustomerName = request.CustomerName,
                CustomerType = request.CustomerType,
                ContactPerson = request.ContactPerson,
                Phone = request.Phone,
                Email = request.Email,
                Website = request.Website,
                Address = request.Address,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country,
                CreditLimit = request.CreditLimit,
                PaymentTerms = request.PaymentTerms,
                CurrentBalance = 0,
                TaxId = request.TaxId,
                Notes = request.Notes,
                IsActive = request.IsActive
            };

            // Store user ID as string for audit trail
            customer.CreatedBy = userId.ToString();
            customer.CreatedAt = DateTime.UtcNow;

            _dbContext.Set<CustomerEntity>().Add(customer);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Edit(UpdateCustomerRequest request, Guid userId)
        {
            var customer = await _dbContext.Set<CustomerEntity>()
                .FirstOrDefaultAsync(x => x.CustomerId == request.CustomerId && !x.IsDeleted);

            if (customer == null)
                throw new Exception("Customer not found");

            // Check if customer code already exists (excluding current customer)
            if (customer.CustomerCode != request.CustomerCode)
            {
                var existingCustomer = await _dbContext.Set<CustomerEntity>()
                    .FirstOrDefaultAsync(x => x.CustomerCode == request.CustomerCode
                        && x.CustomerId != request.CustomerId
                        && !x.IsDeleted);

                if (existingCustomer != null)
                {
                    throw new Exception($"Customer code '{request.CustomerCode}' already exists");
                }
            }

            customer.CustomerCode = request.CustomerCode;
            customer.CustomerName = request.CustomerName;
            customer.CustomerType = request.CustomerType;
            customer.ContactPerson = request.ContactPerson;
            customer.Phone = request.Phone;
            customer.Email = request.Email;
            customer.Website = request.Website;
            customer.Address = request.Address;
            customer.City = request.City;
            customer.State = request.State;
            customer.PostalCode = request.PostalCode;
            customer.Country = request.Country;
            customer.CreditLimit = request.CreditLimit;
            customer.PaymentTerms = request.PaymentTerms;
            customer.TaxId = request.TaxId;
            customer.Notes = request.Notes;
            customer.IsActive = request.IsActive;

            // Store user ID as string for audit trail
            customer.UpdatedBy = userId.ToString();
            customer.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid customerId, Guid userId)
        {
            var customer = await _dbContext.Set<CustomerEntity>()
                .FirstOrDefaultAsync(x => x.CustomerId == customerId && !x.IsDeleted);

            if (customer == null)
                throw new Exception("Customer not found");

            // IMPORTANT: Check if customer has related transactions before deletion
            // TODO: Implement validation to check:
            // 1. Sales Invoices (InvoiceEntity where CustomerId = customerId)
            // 2. Purchase Orders (PurchaseOrderEntity where CustomerId = customerId)
            // 3. Journal Entries (JournalEntryLineEntity where CustomerId = customerId)
            // 4. Payments/Receipts (PaymentEntity where CustomerId = customerId)
            // 5. Outstanding Balance (customer.CurrentBalance != 0)
            // 
            // Example implementation:
            // var hasInvoices = await _dbContext.Set<InvoiceEntity>().AnyAsync(x => x.CustomerId == customerId);
            // if (hasInvoices)
            //     throw new Exception("Cannot delete customer with existing invoices. Please archive instead.");
            //
            // For now, we'll allow deletion (soft delete)

            customer.IsDeleted = true;
            customer.DeletedAt = DateTime.UtcNow;
            customer.DeletedBy = userId.ToString();

            await _dbContext.SaveChangesAsync();
        }

        public async Task ToggleStatus(Guid customerId, Guid userId)
        {
            var customer = await _dbContext.Set<CustomerEntity>()
                .FirstOrDefaultAsync(x => x.CustomerId == customerId && !x.IsDeleted);

            if (customer == null)
                throw new Exception("Customer not found");

            customer.IsActive = !customer.IsActive;
            
            // Store user ID as string for audit trail
            customer.UpdatedBy = userId.ToString();
            customer.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<CustomerDropdownViewModel>> GetActiveCustomersAsync()
        {
            return await _dbContext.Set<CustomerEntity>()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.CustomerCode)
                .Select(x => new CustomerDropdownViewModel
                {
                    CustomerId = x.CustomerId,
                    CustomerCode = x.CustomerCode,
                    CustomerName = x.CustomerName
                })
                .ToListAsync();
        }

        public async Task<bool> IsCodeUniqueAsync(string customerCode, Guid? excludeId = null)
        {
            var query = _dbContext.Set<CustomerEntity>()
                .Where(x => x.CustomerCode == customerCode && !x.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.CustomerId != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<string> GenerateCustomerCodeAsync()
        {
            var lastCustomer = await _dbContext.Set<CustomerEntity>()
                .Where(x => x.CustomerCode.StartsWith("CUST-"))
                .OrderByDescending(x => x.CustomerCode)
                .FirstOrDefaultAsync();

            if (lastCustomer == null)
            {
                return "CUST-00001";
            }

            var lastCode = lastCustomer.CustomerCode;
            var lastNumber = int.Parse(lastCode.Substring(5));
            var newNumber = lastNumber + 1;

            return $"CUST-{newNumber:D5}";
        }

        public async Task<byte[]> ExportToExcelAsync(string? customerType, bool? isActive)
        {
            var query = _dbContext.Set<CustomerEntity>()
                .Where(x => !x.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(customerType))
            {
                query = query.Where(x => x.CustomerType == customerType);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var customers = await query
                .OrderBy(x => x.CustomerCode)
                .ToListAsync();

            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Customers");

            // Header style
            var headerStyle = workbook.CreateCellStyle();
            var headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerStyle.SetFont(headerFont);

            // Create header row
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

            // Data rows
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

            // Auto-size columns
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

