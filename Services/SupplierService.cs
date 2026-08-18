using AccuFlow.Entities.Context;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Supplier;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace AccuFlow.Services
{
    public interface ISupplierService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableSupplierRequest request);
        Task<SupplierViewModel?> GetById(Guid supplierId);
        Task<SupplierViewModel?> GetByCode(string supplierCode);
        Task Create(CreateSupplierRequest request, Guid userId);
        Task Edit(UpdateSupplierRequest request, Guid userId);
        Task Delete(Guid supplierId, Guid userId);
        Task ToggleStatus(Guid supplierId, Guid userId);
        Task<List<SupplierDropdownViewModel>> GetActiveSuppliersAsync();
        Task<bool> IsCodeUniqueAsync(string supplierCode, Guid? excludeId = null);
        Task<string> GenerateSupplierCodeAsync();
        Task<byte[]> ExportToExcelAsync(string? supplierType, bool? isActive);
    }

    public class SupplierService : BaseService, ISupplierService
    {
        public SupplierService(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<BaseDatatableResponse> Datatable(DataTableSupplierRequest request)
        {
            var query = _dbContext.Set<SupplierEntity>()
                .Where(x => !x.IsDeleted)
                .AsQueryable();

            var totalRecord = await query.CountAsync();

            if (!string.IsNullOrEmpty(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x =>
                    x.SupplierCode.ToLower().Contains(search) ||
                    x.SupplierName.ToLower().Contains(search) ||
                    (x.ContactPerson != null && x.ContactPerson.ToLower().Contains(search)) ||
                    (x.Phone != null && x.Phone.ToLower().Contains(search)) ||
                    (x.Email != null && x.Email.ToLower().Contains(search))
                );
            }

            if (!string.IsNullOrEmpty(request.SupplierType))
            {
                query = query.Where(x => x.SupplierType == request.SupplierType);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            query = request?.OrderBy?.ToLower() switch
            {
                "suppliercode" => request?.OrderType?.ToLower() == "asc"
                    ? query.OrderBy(x => x.SupplierCode)
                    : query.OrderByDescending(x => x.SupplierCode),
                "suppliername" => request?.OrderType?.ToLower() == "asc"
                    ? query.OrderBy(x => x.SupplierName)
                    : query.OrderByDescending(x => x.SupplierName),
                "suppliertype" => request?.OrderType?.ToLower() == "asc"
                    ? query.OrderBy(x => x.SupplierType)
                    : query.OrderByDescending(x => x.SupplierType),
                "createdat" => request?.OrderType?.ToLower() == "asc"
                    ? query.OrderBy(x => x.CreatedAt)
                    : query.OrderByDescending(x => x.CreatedAt),
                _ => query.OrderBy(x => x.SupplierCode)
            };

            var totalFiltered = await query.CountAsync();

            var data = await query
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .Select(x => new SupplierViewModel
                {
                    SupplierId = x.SupplierId,
                    SupplierCode = x.SupplierCode,
                    SupplierName = x.SupplierName,
                    SupplierType = x.SupplierType,
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

        public async Task<SupplierViewModel?> GetById(Guid supplierId)
        {
            var supplier = await _dbContext.Set<SupplierEntity>()
                .Where(x => x.SupplierId == supplierId && !x.IsDeleted)
                .Select(x => new SupplierViewModel
                {
                    SupplierId = x.SupplierId,
                    SupplierCode = x.SupplierCode,
                    SupplierName = x.SupplierName,
                    SupplierType = x.SupplierType,
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

            if (supplier != null)
            {
                // Resolve user names from user IDs
                supplier.CreatedBy = await GetUserNameFromIdAsync(supplier.CreatedBy ?? string.Empty);
                if (!string.IsNullOrEmpty(supplier.UpdatedBy))
                {
                    supplier.UpdatedBy = await GetUserNameFromIdAsync(supplier.UpdatedBy);
                }
            }

            return supplier;
        }

        public async Task<SupplierViewModel?> GetByCode(string supplierCode)
        {
            return await _dbContext.Set<SupplierEntity>()
                .Where(x => x.SupplierCode == supplierCode && !x.IsDeleted)
                .Select(x => new SupplierViewModel
                {
                    SupplierId = x.SupplierId,
                    SupplierCode = x.SupplierCode,
                    SupplierName = x.SupplierName,
                    SupplierType = x.SupplierType,
                    ContactPerson = x.ContactPerson,
                    Phone = x.Phone,
                    Email = x.Email,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync();
        }

        public async Task Create(CreateSupplierRequest request, Guid userId)
        {
            // Check if supplier code already exists
            var existingSupplier = await _dbContext.Set<SupplierEntity>()
                .FirstOrDefaultAsync(x => x.SupplierCode == request.SupplierCode && !x.IsDeleted);

            if (existingSupplier != null)
            {
                throw new Exception($"Supplier code '{request.SupplierCode}' already exists");
            }

            var supplier = new SupplierEntity
            {
                SupplierId = Guid.NewGuid(),
                SupplierCode = request.SupplierCode,
                SupplierName = request.SupplierName,
                SupplierType = request.SupplierType,
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
            supplier.CreatedBy = userId.ToString();
            supplier.CreatedAt = DateTime.UtcNow;

            _dbContext.Set<SupplierEntity>().Add(supplier);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Edit(UpdateSupplierRequest request, Guid userId)
        {
            var supplier = await _dbContext.Set<SupplierEntity>()
                .FirstOrDefaultAsync(x => x.SupplierId == request.SupplierId && !x.IsDeleted);

            if (supplier == null)
                throw new Exception("Supplier not found");

            // Check if supplier code already exists (excluding current supplier)
            if (supplier.SupplierCode != request.SupplierCode)
            {
                var existingSupplier = await _dbContext.Set<SupplierEntity>()
                    .FirstOrDefaultAsync(x => x.SupplierCode == request.SupplierCode
                        && x.SupplierId != request.SupplierId
                        && !x.IsDeleted);

                if (existingSupplier != null)
                {
                    throw new Exception($"Supplier code '{request.SupplierCode}' already exists");
                }
            }

            supplier.SupplierCode = request.SupplierCode;
            supplier.SupplierName = request.SupplierName;
            supplier.SupplierType = request.SupplierType;
            supplier.ContactPerson = request.ContactPerson;
            supplier.Phone = request.Phone;
            supplier.Email = request.Email;
            supplier.Website = request.Website;
            supplier.Address = request.Address;
            supplier.City = request.City;
            supplier.State = request.State;
            supplier.PostalCode = request.PostalCode;
            supplier.Country = request.Country;
            supplier.CreditLimit = request.CreditLimit;
            supplier.PaymentTerms = request.PaymentTerms;
            supplier.TaxId = request.TaxId;
            supplier.Notes = request.Notes;
            supplier.IsActive = request.IsActive;

            // Store user ID as string for audit trail
            supplier.UpdatedBy = userId.ToString();
            supplier.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid supplierId, Guid userId)
        {
            var supplier = await _dbContext.Set<SupplierEntity>()
                .FirstOrDefaultAsync(x => x.SupplierId == supplierId && !x.IsDeleted);

            if (supplier == null)
                throw new Exception("Supplier not found");

            // IMPORTANT: Check if supplier has related transactions before deletion
            // TODO: Implement validation to check:
            // 1. Sales Invoices (InvoiceEntity where SupplierId = supplierId)
            // 2. Purchase Orders (PurchaseOrderEntity where SupplierId = supplierId)
            // 3. Journal Entries (JournalEntryLineEntity where SupplierId = supplierId)
            // 4. Payments/Receipts (PaymentEntity where SupplierId = supplierId)
            // 5. Outstanding Balance (supplier.CurrentBalance != 0)
            // 
            // Example implementation:
            // var hasInvoices = await _dbContext.Set<InvoiceEntity>().AnyAsync(x => x.SupplierId == supplierId);
            // if (hasInvoices)
            //     throw new Exception("Cannot delete supplier with existing invoices. Please archive instead.");
            //
            // For now, we'll allow deletion (soft delete)

            supplier.IsDeleted = true;
            supplier.DeletedAt = DateTime.UtcNow;
            supplier.DeletedBy = userId.ToString();

            await _dbContext.SaveChangesAsync();
        }

        public async Task ToggleStatus(Guid supplierId, Guid userId)
        {
            var supplier = await _dbContext.Set<SupplierEntity>()
                .FirstOrDefaultAsync(x => x.SupplierId == supplierId && !x.IsDeleted);

            if (supplier == null)
                throw new Exception("Supplier not found");

            supplier.IsActive = !supplier.IsActive;
            
            // Store user ID as string for audit trail
            supplier.UpdatedBy = userId.ToString();
            supplier.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<SupplierDropdownViewModel>> GetActiveSuppliersAsync()
        {
            return await _dbContext.Set<SupplierEntity>()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.SupplierCode)
                .Select(x => new SupplierDropdownViewModel
                {
                    SupplierId = x.SupplierId,
                    SupplierCode = x.SupplierCode,
                    SupplierName = x.SupplierName
                })
                .ToListAsync();
        }

        public async Task<bool> IsCodeUniqueAsync(string supplierCode, Guid? excludeId = null)
        {
            var query = _dbContext.Set<SupplierEntity>()
                .Where(x => x.SupplierCode == supplierCode && !x.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.SupplierId != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<string> GenerateSupplierCodeAsync()
        {
            var lastSupplier = await _dbContext.Set<SupplierEntity>()
                .Where(x => x.SupplierCode.StartsWith("SUPP-"))
                .OrderByDescending(x => x.SupplierCode)
                .FirstOrDefaultAsync();

            if (lastSupplier == null)
            {
                return "SUPP-00001";
            }

            var lastCode = lastSupplier.SupplierCode;
            var lastNumber = int.Parse(lastCode.Substring(5));
            var newNumber = lastNumber + 1;

            return $"SUPP-{newNumber:D5}";
        }

        public async Task<byte[]> ExportToExcelAsync(string? supplierType, bool? isActive)
        {
            var query = _dbContext.Set<SupplierEntity>()
                .Where(x => !x.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(supplierType))
            {
                query = query.Where(x => x.SupplierType == supplierType);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var suppliers = await query
                .OrderBy(x => x.SupplierCode)
                .ToListAsync();

            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Suppliers");

            // Header style
            var headerStyle = workbook.CreateCellStyle();
            var headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerStyle.SetFont(headerFont);

            // Create header row
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

            // Data rows
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

