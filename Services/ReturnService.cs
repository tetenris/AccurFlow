using AccuFlow.Infrastructure.Persistence;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.JournalEntry;
using AccuFlow.Models.Return;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface IReturnService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableReturnRequest request);
        Task<ReturnDetailViewModel?> GetById(Guid id);
        Task<List<WarehouseEntity>> GetWarehouses();
        Task<List<ReturnInvoiceOptionViewModel>> GetInvoices(string returnType);
        Task<ReturnInvoiceViewModel> GetInvoiceLines(Guid invoiceId);
        Task Create(CreateReturnRequest request, Guid userId);
        Task Update(UpdateReturnRequest request, Guid userId);
        Task Post(Guid id, Guid userId);
        Task Delete(Guid id, Guid userId);
    }

    public class ReturnService : BaseService, IReturnService
    {
        private static readonly Guid SalesRevenueAccountId = Guid.Parse("40000000-0000-0000-0000-000000000002");
        private static readonly Guid TaxPayableAccountId = Guid.Parse("20000000-0000-0000-0000-000000000003");
        private static readonly Guid AccountsReceivableAccountId = Guid.Parse("10000000-0000-0000-0000-000000000004");
        private static readonly Guid AccountsPayableAccountId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        private static readonly Guid DefaultInventoryAccountId = Guid.Parse("10000000-0000-0000-0000-000000000005");

        private readonly IJournalEntryService _journalEntryService;

        public ReturnService(AppDbContext dbContext, IJournalEntryService journalEntryService) : base(dbContext)
        {
            _journalEntryService = journalEntryService;
        }

        public async Task<BaseDatatableResponse> Datatable(DataTableReturnRequest request)
        {
            var query = _dbContext.GoodsReturns
                .Include(x => x.Invoice)
                .Include(x => x.Customer)
                .Include(x => x.Supplier)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.ReturnType)) query = query.Where(x => x.ReturnType == request.ReturnType);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.GoodsReturnNumber.ToLower().Contains(search) || x.Invoice.InvoiceNumber.ToLower().Contains(search));
            }

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.ReturnDate)
                .Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new ReturnViewModel
                {
                    GoodsReturnId = x.GoodsReturnId,
                    GoodsReturnNumber = x.GoodsReturnNumber,
                    ReturnType = x.ReturnType,
                    ReturnDate = x.ReturnDate,
                    InvoiceNumber = x.Invoice.InvoiceNumber,
                    PartnerName = x.Customer != null ? x.Customer.CustomerName : x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount
                }).ToListAsync();

            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<ReturnDetailViewModel?> GetById(Guid id)
        {
            return await _dbContext.GoodsReturns
                .Include(x => x.Invoice)
                .Include(x => x.Customer)
                .Include(x => x.Supplier)
                .Include(x => x.Warehouse)
                .Include(x => x.JournalEntry)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.GoodsReturnId == id && !x.IsDeleted)
                .Select(x => new ReturnDetailViewModel
                {
                    GoodsReturnId = x.GoodsReturnId,
                    GoodsReturnNumber = x.GoodsReturnNumber,
                    ReturnType = x.ReturnType,
                    ReturnDate = x.ReturnDate,
                    InvoiceNumber = x.Invoice.InvoiceNumber,
                    PartnerName = x.Customer != null ? x.Customer.CustomerName : x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount,
                    WarehouseId = x.WarehouseId,
                    WarehouseName = x.Warehouse.WarehouseName,
                    Notes = x.Notes,
                    JournalNumber = x.JournalEntry != null ? x.JournalEntry.JournalNumber : null,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanPost = x.Status == "Draft",
                    Lines = x.Lines.Select(l => new ReturnLineViewModel
                    {
                        GoodsReturnLineId = l.GoodsReturnLineId,
                        InvoiceLineId = l.InvoiceLineId,
                        ItemId = l.ItemId,
                        ItemCode = l.Item != null ? l.Item.ItemCode : string.Empty,
                        ItemName = l.Item != null ? l.Item.ItemName : string.Empty,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        TaxAmount = l.TaxAmount,
                        LineTotal = l.LineTotal
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task<List<WarehouseEntity>> GetWarehouses()
        {
            return await _dbContext.Warehouses
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.WarehouseCode)
                .ToListAsync();
        }

        public async Task<List<ReturnInvoiceOptionViewModel>> GetInvoices(string returnType)
        {
            var returnedByLine = await GetReturnedByInvoiceLineAsync();

            var invoices = await _dbContext.Invoices
                .Include(x => x.Customer)
                .Include(x => x.Supplier)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .Where(x => x.InvoiceType == returnType && x.Status == "Posted" && !x.IsDeleted)
                .OrderByDescending(x => x.InvoiceDate)
                .Select(x => new
                {
                    x.InvoiceId,
                    x.InvoiceNumber,
                    x.InvoiceDate,
                    x.TotalAmount,
                    PartnerName = x.Customer != null ? x.Customer.CustomerName : x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                    Lines = x.Lines.Select(l => new { l.InvoiceLineId, l.Quantity }).ToList()
                })
                .ToListAsync();

            var result = new List<ReturnInvoiceOptionViewModel>();
            foreach (var inv in invoices)
            {
                var remaining = inv.Lines.Sum(l =>
                {
                    var returnedQty = returnedByLine.GetValueOrDefault(l.InvoiceLineId, 0);
                    return Math.Max(0, l.Quantity - returnedQty);
                });
                if (remaining <= 0) continue;
                result.Add(new ReturnInvoiceOptionViewModel
                {
                    InvoiceId = inv.InvoiceId,
                    InvoiceNumber = inv.InvoiceNumber,
                    PartnerName = inv.PartnerName,
                    InvoiceDate = inv.InvoiceDate,
                    TotalAmount = inv.TotalAmount
                });
            }
            return result;
        }

        public async Task<ReturnInvoiceViewModel> GetInvoiceLines(Guid invoiceId)
        {
            var invoice = await _dbContext.Invoices
                .Include(x => x.Customer)
                .Include(x => x.Supplier)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .FirstOrDefaultAsync(x => x.InvoiceId == invoiceId && !x.IsDeleted);
            if (invoice == null) throw new Exception("Invoice not found");

            var returnedByLine = await GetReturnedByInvoiceLineAsync();
            var lines = invoice.Lines.Where(l => !l.IsDeleted).Select(l =>
            {
                var returnedQty = returnedByLine.GetValueOrDefault(l.InvoiceLineId, 0);
                return new ReturnInvoiceLineViewModel
                {
                    InvoiceLineId = l.InvoiceLineId,
                    ItemId = l.ItemId,
                    ItemCode = l.Item != null ? l.Item.ItemCode : string.Empty,
                    ItemName = l.Item != null ? l.Item.ItemName : string.Empty,
                    Description = l.Description,
                    Quantity = l.Quantity,
                    ReturnedQuantity = returnedQty,
                    RemainingQuantity = l.Quantity - returnedQty,
                    UnitPrice = l.UnitPrice,
                    TaxAmount = l.TaxAmount
                };
            })
            .Where(x => x.RemainingQuantity > 0)
            .ToList();

            return new ReturnInvoiceViewModel
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceType = invoice.InvoiceType,
                CustomerId = invoice.CustomerId,
                SupplierId = invoice.SupplierId,
                PartnerName = invoice.Customer != null ? invoice.Customer.CustomerName : invoice.Supplier != null ? invoice.Supplier.SupplierName : string.Empty,
                InvoiceDate = invoice.InvoiceDate,
                Lines = lines
            };
        }

        public async Task Create(CreateReturnRequest request, Guid userId)
        {
            if (request.InvoiceId == Guid.Empty) throw new Exception("Invoice is required");
            if (request.WarehouseId == Guid.Empty) throw new Exception("Warehouse is required");
            if (request.Lines == null || request.Lines.Count == 0) throw new Exception("At least one item line is required");

            var invoice = await _dbContext.Invoices
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.InvoiceId == request.InvoiceId && !x.IsDeleted);
            if (invoice == null) throw new Exception("Invoice not found");
            if (invoice.InvoiceType != request.ReturnType) throw new Exception($"Return type must match invoice type ({invoice.InvoiceType})");
            if (invoice.Status == "Draft") throw new Exception("Invoice must be posted before it can be returned");
            if (invoice.Status == "Cancelled") throw new Exception("Cancelled invoice cannot be returned");

            var invoiceLineIds = invoice.Lines.Select(l => l.InvoiceLineId).ToHashSet();
            if (request.Lines.Any(l => !invoiceLineIds.Contains(l.InvoiceLineId)))
                throw new Exception("Item line does not belong to the selected invoice");

            var warehouse = await _dbContext.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == request.WarehouseId && !x.IsDeleted);
            if (warehouse == null) throw new Exception("Warehouse not found");

            var returnDoc = new GoodsReturnEntity
            {
                GoodsReturnId = Guid.NewGuid(),
                GoodsReturnNumber = await GenerateNumberAsync(request.ReturnType),
                ReturnType = request.ReturnType,
                ReturnDate = request.ReturnDate,
                InvoiceId = request.InvoiceId,
                CustomerId = invoice.CustomerId,
                SupplierId = invoice.SupplierId,
                WarehouseId = request.WarehouseId,
                Status = "Draft",
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            };

            foreach (var line in request.Lines)
            {
                if (line.Quantity <= 0) continue;
                var lineTotal = (line.Quantity * line.UnitPrice) + line.TaxAmount;
                returnDoc.Lines.Add(new GoodsReturnLineEntity
                {
                    GoodsReturnLineId = Guid.NewGuid(),
                    InvoiceLineId = line.InvoiceLineId,
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                returnDoc.SubTotal += line.Quantity * line.UnitPrice;
                returnDoc.TaxAmount += line.TaxAmount;
                returnDoc.TotalAmount += lineTotal;
            }

            if (returnDoc.Lines.Count == 0) throw new Exception("At least one item line is required");

            _dbContext.GoodsReturns.Add(returnDoc);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(UpdateReturnRequest request, Guid userId)
        {
            var returnDoc = await _dbContext.GoodsReturns
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.GoodsReturnId == request.GoodsReturnId && !x.IsDeleted);
            if (returnDoc == null) throw new Exception("Return not found");
            if (returnDoc.Status != "Draft") throw new Exception("Only draft return can be edited");

            var invoice = await _dbContext.Invoices
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.InvoiceId == request.InvoiceId && !x.IsDeleted);
            if (invoice == null) throw new Exception("Invoice not found");
            if (invoice.InvoiceType != request.ReturnType) throw new Exception($"Return type must match invoice type ({invoice.InvoiceType})");

            returnDoc.ReturnType = request.ReturnType;
            returnDoc.ReturnDate = request.ReturnDate;
            returnDoc.InvoiceId = request.InvoiceId;
            returnDoc.CustomerId = invoice.CustomerId;
            returnDoc.SupplierId = invoice.SupplierId;
            returnDoc.WarehouseId = request.WarehouseId;
            returnDoc.Notes = request.Notes;
            returnDoc.SubTotal = 0;
            returnDoc.TaxAmount = 0;
            returnDoc.TotalAmount = 0;
            returnDoc.UpdatedAt = DateTime.UtcNow;
            returnDoc.UpdatedBy = userId.ToString();

            foreach (var existing in returnDoc.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true;
                existing.DeletedAt = DateTime.UtcNow;
                existing.DeletedBy = userId.ToString();
            }

            foreach (var line in request.Lines)
            {
                if (line.Quantity <= 0) continue;
                var lineTotal = (line.Quantity * line.UnitPrice) + line.TaxAmount;
                returnDoc.Lines.Add(new GoodsReturnLineEntity
                {
                    GoodsReturnLineId = Guid.NewGuid(),
                    InvoiceLineId = line.InvoiceLineId,
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                returnDoc.SubTotal += line.Quantity * line.UnitPrice;
                returnDoc.TaxAmount += line.TaxAmount;
                returnDoc.TotalAmount += lineTotal;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task Post(Guid id, Guid userId)
        {
            var returnDoc = await _dbContext.GoodsReturns
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.GoodsReturnId == id && !x.IsDeleted);
            if (returnDoc == null) throw new Exception("Return not found");
            if (returnDoc.Status == "Posted") throw new Exception("Return is already posted");
            if (returnDoc.Status != "Draft") throw new Exception("Only draft return can be posted");

            var returnedByLine = await GetReturnedByInvoiceLineAsync();
            foreach (var line in returnDoc.Lines)
            {
                var invoiceLine = await _dbContext.InvoiceLines.FirstOrDefaultAsync(x => x.InvoiceLineId == line.InvoiceLineId && !x.IsDeleted);
                if (invoiceLine == null) throw new Exception($"Invoice line not found for {line.Description}");
                var alreadyReturned = returnedByLine.GetValueOrDefault(line.InvoiceLineId, 0);
                if (line.Quantity + alreadyReturned > invoiceLine.Quantity)
                    throw new Exception($"Returned quantity for {line.Description} exceeds invoice quantity (remaining: {invoiceLine.Quantity - alreadyReturned})");
            }

            foreach (var line in returnDoc.Lines)
            {
                if (!line.ItemId.HasValue) continue;
                _dbContext.StockMovements.Add(new StockMovementEntity
                {
                    StockMovementId = Guid.NewGuid(),
                    MovementDate = returnDoc.ReturnDate,
                    ItemId = line.ItemId.Value,
                    WarehouseId = returnDoc.WarehouseId,
                    MovementType = returnDoc.ReturnType == "Sales" ? "Sales Return" : "Purchase Return",
                    SourceDocumentType = "GoodsReturn",
                    SourceDocumentId = returnDoc.GoodsReturnId,
                    QuantityIn = 0,
                    QuantityOut = line.Quantity,
                    UnitCost = line.UnitPrice,
                    Notes = $"Return {returnDoc.GoodsReturnNumber}",
                    CreatedBy = userId.ToString()
                });
            }

            var journalId = await CreateReturnJournal(returnDoc, userId);
            returnDoc.JournalId = journalId;
            returnDoc.Status = "Posted";
            returnDoc.UpdatedAt = DateTime.UtcNow;
            returnDoc.UpdatedBy = userId.ToString();

            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var returnDoc = await _dbContext.GoodsReturns.FirstOrDefaultAsync(x => x.GoodsReturnId == id && !x.IsDeleted);
            if (returnDoc == null) throw new Exception("Return not found");
            if (returnDoc.Status != "Draft") throw new Exception("Only draft return can be deleted");

            returnDoc.IsDeleted = true;
            returnDoc.DeletedAt = DateTime.UtcNow;
            returnDoc.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        private async Task<Guid> CreateReturnJournal(GoodsReturnEntity returnDoc, Guid userId)
        {
            var description = $"Auto journal for {returnDoc.GoodsReturnNumber}";
            var journalLines = new List<JournalLineRequest>();

            if (returnDoc.ReturnType == "Sales")
            {
                journalLines.Add(new JournalLineRequest
                {
                    AccountId = SalesRevenueAccountId,
                    Description = description,
                    DebitAmount = returnDoc.SubTotal,
                    CreditAmount = 0
                });
                if (returnDoc.TaxAmount > 0)
                {
                    journalLines.Add(new JournalLineRequest
                    {
                        AccountId = TaxPayableAccountId,
                        Description = description,
                        DebitAmount = returnDoc.TaxAmount,
                        CreditAmount = 0
                    });
                }
                journalLines.Add(new JournalLineRequest
                {
                    AccountId = AccountsReceivableAccountId,
                    Description = description,
                    DebitAmount = 0,
                    CreditAmount = returnDoc.TotalAmount
                });
            }
            else
            {
                journalLines.Add(new JournalLineRequest
                {
                    AccountId = AccountsPayableAccountId,
                    Description = description,
                    DebitAmount = returnDoc.TotalAmount,
                    CreditAmount = 0
                });

                var inventoryByAccount = new Dictionary<Guid, decimal>();
                foreach (var line in returnDoc.Lines)
                {
                    var accountId = await GetItemInventoryAccountIdAsync(line.ItemId);
                    var amount = line.Quantity * line.UnitPrice;
                    if (!inventoryByAccount.ContainsKey(accountId)) inventoryByAccount[accountId] = 0;
                    inventoryByAccount[accountId] += amount;
                }
                foreach (var item in inventoryByAccount)
                {
                    journalLines.Add(new JournalLineRequest
                    {
                        AccountId = item.Key,
                        Description = description,
                        DebitAmount = 0,
                        CreditAmount = item.Value
                    });
                }

                if (returnDoc.TaxAmount > 0)
                {
                    journalLines.Add(new JournalLineRequest
                    {
                        AccountId = TaxPayableAccountId,
                        Description = description,
                        DebitAmount = 0,
                        CreditAmount = returnDoc.TaxAmount
                    });
                }
            }

            var journalId = await _journalEntryService.CreateAsync(new CreateJournalEntryRequest
            {
                JournalDate = returnDoc.ReturnDate,
                Description = description,
                JournalLines = journalLines
            }, userId);

            await _journalEntryService.PostAsync(new PostJournalRequest
            {
                JournalId = journalId,
                PostedDate = returnDoc.ReturnDate
            }, userId);

            return journalId;
        }

        private async Task<Guid> GetItemInventoryAccountIdAsync(Guid? itemId)
        {
            if (!itemId.HasValue) return DefaultInventoryAccountId;
            var accountId = await _dbContext.Items
                .Where(x => x.ItemId == itemId.Value && !x.IsDeleted)
                .Select(x => x.InventoryAccountId)
                .FirstOrDefaultAsync();
            return accountId ?? DefaultInventoryAccountId;
        }

        private async Task<Dictionary<Guid, decimal>> GetReturnedByInvoiceLineAsync()
        {
            return await _dbContext.GoodsReturnLines
                .Where(x => !x.IsDeleted && x.GoodsReturn != null && !x.GoodsReturn.IsDeleted && x.GoodsReturn.Status == "Posted")
                .GroupBy(x => x.InvoiceLineId)
                .Select(g => new { InvoiceLineId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.InvoiceLineId, x => x.Quantity);
        }

        private async Task<string> GenerateNumberAsync(string returnType)
        {
            var prefix = returnType == "Purchase" ? "PR" : "SR";
            var last = await _dbContext.GoodsReturns.Where(x => x.GoodsReturnNumber.StartsWith(prefix + "-"))
                .OrderByDescending(x => x.GoodsReturnNumber).Select(x => x.GoodsReturnNumber).FirstOrDefaultAsync();
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[(prefix.Length + 1)..]) + 1;
            return $"{prefix}-{next:D5}";
        }
    }
}

