using AccuFlow.Entities.Context;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.GoodsReceipt;
using AccuFlow.Models.JournalEntry;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface IGoodsReceiptService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableGoodsReceiptRequest request);
        Task<GoodsReceiptDetailViewModel?> GetById(Guid id);
        Task<List<WarehouseEntity>> GetWarehouses();
        Task<List<PurchaseOrderOptionViewModel>> GetPurchaseOrders();
        Task<PurchaseOrderReceiptViewModel> GetPurchaseOrderLines(Guid purchaseOrderId);
        Task Create(CreateGoodsReceiptRequest request, Guid userId);
        Task Update(UpdateGoodsReceiptRequest request, Guid userId);
        Task Post(Guid id, Guid userId);
        Task Delete(Guid id, Guid userId);
    }

    public class GoodsReceiptService : BaseService, IGoodsReceiptService
    {
        private static readonly Guid AccountsPayableAccountId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        private static readonly Guid DefaultInventoryAccountId = Guid.Parse("10000000-0000-0000-0000-000000000005");

        private readonly IJournalEntryService _journalEntryService;

        public GoodsReceiptService(AppDbContext dbContext, IJournalEntryService journalEntryService) : base(dbContext)
        {
            _journalEntryService = journalEntryService;
        }

        public async Task<BaseDatatableResponse> Datatable(DataTableGoodsReceiptRequest request)
        {
            var query = _dbContext.GoodsReceipts
                .Include(x => x.Supplier)
                .Include(x => x.Warehouse)
                .Include(x => x.PurchaseOrder)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.GoodsReceiptNumber.ToLower().Contains(search)
                    || x.PurchaseOrder.PurchaseOrderNumber.ToLower().Contains(search)
                    || x.Supplier.SupplierName.ToLower().Contains(search));
            }

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.ReceiptDate)
                .Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new GoodsReceiptViewModel
                {
                    GoodsReceiptId = x.GoodsReceiptId,
                    GoodsReceiptNumber = x.GoodsReceiptNumber,
                    ReceiptDate = x.ReceiptDate,
                    PurchaseOrderNumber = x.PurchaseOrder.PurchaseOrderNumber,
                    SupplierName = x.Supplier.SupplierName,
                    WarehouseName = x.Warehouse.WarehouseName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount
                }).ToListAsync();

            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<GoodsReceiptDetailViewModel?> GetById(Guid id)
        {
            return await _dbContext.GoodsReceipts
                .Include(x => x.Supplier)
                .Include(x => x.Warehouse)
                .Include(x => x.PurchaseOrder)
                .Include(x => x.JournalEntry)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.GoodsReceiptId == id && !x.IsDeleted)
                .Select(x => new GoodsReceiptDetailViewModel
                {
                    GoodsReceiptId = x.GoodsReceiptId,
                    GoodsReceiptNumber = x.GoodsReceiptNumber,
                    ReceiptDate = x.ReceiptDate,
                    PurchaseOrderId = x.PurchaseOrderId,
                    PurchaseOrderNumber = x.PurchaseOrder.PurchaseOrderNumber,
                    SupplierName = x.Supplier.SupplierName,
                    WarehouseName = x.Warehouse.WarehouseName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount,
                    Notes = x.Notes,
                    JournalNumber = x.JournalEntry != null ? x.JournalEntry.JournalNumber : null,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanPost = x.Status == "Draft",
                    Lines = x.Lines.Select(l => new GoodsReceiptLineViewModel
                    {
                        GoodsReceiptLineId = l.GoodsReceiptLineId,
                        PurchaseOrderLineId = l.PurchaseOrderLineId,
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

        public async Task<List<PurchaseOrderOptionViewModel>> GetPurchaseOrders()
        {
            var receivedByLine = await GetReceivedByPoLineAsync();
            var pos = await _dbContext.PurchaseOrders
                .Include(x => x.Supplier)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .Where(x => x.Status == "Approved" && !x.IsDeleted)
                .OrderByDescending(x => x.OrderDate)
                .Select(x => new
                {
                    x.PurchaseOrderId,
                    x.PurchaseOrderNumber,
                    x.Supplier.SupplierName,
                    x.OrderDate,
                    x.TotalAmount,
                    Lines = x.Lines.Select(l => new { l.PurchaseOrderLineId, l.Quantity }).ToList()
                })
                .ToListAsync();

            var result = new List<PurchaseOrderOptionViewModel>();
            foreach (var po in pos)
            {
                var remaining = po.Lines.Sum(l =>
                {
                    var receivedQty = receivedByLine.GetValueOrDefault(l.PurchaseOrderLineId, 0);
                    return Math.Max(0, l.Quantity - receivedQty);
                });
                if (remaining <= 0) continue;
                result.Add(new PurchaseOrderOptionViewModel
                {
                    PurchaseOrderId = po.PurchaseOrderId,
                    PurchaseOrderNumber = po.PurchaseOrderNumber,
                    SupplierName = po.SupplierName,
                    OrderDate = po.OrderDate,
                    TotalAmount = po.TotalAmount
                });
            }
            return result;
        }

        public async Task<PurchaseOrderReceiptViewModel> GetPurchaseOrderLines(Guid purchaseOrderId)
        {
            var po = await _dbContext.PurchaseOrders
                .Include(x => x.Supplier)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .FirstOrDefaultAsync(x => x.PurchaseOrderId == purchaseOrderId && !x.IsDeleted);
            if (po == null) throw new Exception("Purchase order not found");

            var receivedByLine = await GetReceivedByPoLineAsync();
            var lines = po.Lines.Where(l => !l.IsDeleted).Select(l =>
            {
                var receivedQty = receivedByLine.GetValueOrDefault(l.PurchaseOrderLineId, 0);
                return new PurchaseOrderReceiptLineViewModel
                {
                    PurchaseOrderLineId = l.PurchaseOrderLineId,
                    ItemId = l.ItemId,
                    ItemCode = l.Item != null ? l.Item.ItemCode : string.Empty,
                    ItemName = l.Item != null ? l.Item.ItemName : string.Empty,
                    Description = l.Description,
                    Quantity = l.Quantity,
                    ReceivedQuantity = receivedQty,
                    RemainingQuantity = l.Quantity - receivedQty,
                    UnitPrice = l.UnitPrice,
                    TaxAmount = l.TaxAmount
                };
            })
            .Where(x => x.RemainingQuantity > 0)
            .ToList();

            return new PurchaseOrderReceiptViewModel
            {
                PurchaseOrderId = po.PurchaseOrderId,
                PurchaseOrderNumber = po.PurchaseOrderNumber,
                SupplierName = po.Supplier.SupplierName,
                OrderDate = po.OrderDate,
                Lines = lines
            };
        }

        public async Task Create(CreateGoodsReceiptRequest request, Guid userId)
        {
            if (request.PurchaseOrderId == Guid.Empty) throw new Exception("Purchase order is required");
            if (request.WarehouseId == Guid.Empty) throw new Exception("Warehouse is required");
            if (request.Lines == null || request.Lines.Count == 0) throw new Exception("At least one item line is required");

            var po = await _dbContext.PurchaseOrders
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.PurchaseOrderId == request.PurchaseOrderId && !x.IsDeleted);
            if (po == null) throw new Exception("Purchase order not found");
            if (po.Status != "Approved") throw new Exception("Only approved purchase order can be received");

            var poLineIds = po.Lines.Select(l => l.PurchaseOrderLineId).ToHashSet();
            if (request.Lines.Any(l => !poLineIds.Contains(l.PurchaseOrderLineId)))
                throw new Exception("Item line does not belong to the selected purchase order");

            var warehouse = await _dbContext.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == request.WarehouseId && !x.IsDeleted);
            if (warehouse == null) throw new Exception("Warehouse not found");

            var grn = new GoodsReceiptEntity
            {
                GoodsReceiptId = Guid.NewGuid(),
                GoodsReceiptNumber = await GenerateNumberAsync(),
                ReceiptDate = request.ReceiptDate,
                PurchaseOrderId = request.PurchaseOrderId,
                SupplierId = po.SupplierId,
                WarehouseId = request.WarehouseId,
                Status = "Draft",
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            };

            foreach (var line in request.Lines)
            {
                if (line.Quantity <= 0) continue;
                var lineTotal = (line.Quantity * line.UnitPrice) + line.TaxAmount;
                grn.Lines.Add(new GoodsReceiptLineEntity
                {
                    GoodsReceiptLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    PurchaseOrderLineId = line.PurchaseOrderLineId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                grn.SubTotal += line.Quantity * line.UnitPrice;
                grn.TaxAmount += line.TaxAmount;
                grn.TotalAmount += lineTotal;
            }

            if (grn.Lines.Count == 0) throw new Exception("At least one item line is required");

            _dbContext.GoodsReceipts.Add(grn);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(UpdateGoodsReceiptRequest request, Guid userId)
        {
            var grn = await _dbContext.GoodsReceipts
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.GoodsReceiptId == request.GoodsReceiptId && !x.IsDeleted);
            if (grn == null) throw new Exception("Goods receipt not found");
            if (grn.Status != "Draft") throw new Exception("Only draft goods receipt can be edited");

            var po = await _dbContext.PurchaseOrders
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.PurchaseOrderId == request.PurchaseOrderId && !x.IsDeleted);
            if (po == null) throw new Exception("Purchase order not found");
            if (po.Status != "Approved") throw new Exception("Only approved purchase order can be received");

            grn.PurchaseOrderId = request.PurchaseOrderId;
            grn.SupplierId = po.SupplierId;
            grn.WarehouseId = request.WarehouseId;
            grn.ReceiptDate = request.ReceiptDate;
            grn.Notes = request.Notes;
            grn.SubTotal = 0;
            grn.TaxAmount = 0;
            grn.TotalAmount = 0;
            grn.UpdatedAt = DateTime.UtcNow;
            grn.UpdatedBy = userId.ToString();

            foreach (var existing in grn.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true;
                existing.DeletedAt = DateTime.UtcNow;
                existing.DeletedBy = userId.ToString();
            }

            foreach (var line in request.Lines)
            {
                if (line.Quantity <= 0) continue;
                var lineTotal = (line.Quantity * line.UnitPrice) + line.TaxAmount;
                grn.Lines.Add(new GoodsReceiptLineEntity
                {
                    GoodsReceiptLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    PurchaseOrderLineId = line.PurchaseOrderLineId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                grn.SubTotal += line.Quantity * line.UnitPrice;
                grn.TaxAmount += line.TaxAmount;
                grn.TotalAmount += lineTotal;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task Post(Guid id, Guid userId)
        {
            var grn = await _dbContext.GoodsReceipts
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.GoodsReceiptId == id && !x.IsDeleted);
            if (grn == null) throw new Exception("Goods receipt not found");
            if (grn.Status == "Posted") throw new Exception("Goods receipt is already posted");
            if (grn.Status != "Draft") throw new Exception("Only draft goods receipt can be posted");

            var receivedByLine = await GetReceivedByPoLineAsync();
            foreach (var line in grn.Lines)
            {
                var poLine = await _dbContext.PurchaseOrderLines.FirstOrDefaultAsync(x => x.PurchaseOrderLineId == line.PurchaseOrderLineId && !x.IsDeleted);
                if (poLine == null) throw new Exception($"Purchase order line not found for {line.Description}");
                var alreadyReceived = receivedByLine.GetValueOrDefault(line.PurchaseOrderLineId, 0);
                if (line.Quantity + alreadyReceived > poLine.Quantity)
                    throw new Exception($"Received quantity for {line.Description} exceeds remaining order quantity (remaining: {poLine.Quantity - alreadyReceived})");
            }

            foreach (var line in grn.Lines)
            {
                var unitCost = line.UnitPrice;
                _dbContext.StockMovements.Add(new StockMovementEntity
                {
                    StockMovementId = Guid.NewGuid(),
                    MovementDate = grn.ReceiptDate,
                    ItemId = line.ItemId ?? Guid.Empty,
                    WarehouseId = grn.WarehouseId,
                    MovementType = "Goods Received",
                    SourceDocumentType = "GoodsReceipt",
                    SourceDocumentId = grn.GoodsReceiptId,
                    QuantityIn = line.Quantity,
                    QuantityOut = 0,
                    UnitCost = unitCost,
                    Notes = $"Goods receipt {grn.GoodsReceiptNumber}",
                    CreatedBy = userId.ToString()
                });
            }

            var journalId = await CreateGoodsReceiptJournal(grn, userId);
            grn.JournalId = journalId;
            grn.Status = "Posted";
            grn.UpdatedAt = DateTime.UtcNow;
            grn.UpdatedBy = userId.ToString();

            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var grn = await _dbContext.GoodsReceipts.FirstOrDefaultAsync(x => x.GoodsReceiptId == id && !x.IsDeleted);
            if (grn == null) throw new Exception("Goods receipt not found");
            if (grn.Status != "Draft") throw new Exception("Only draft goods receipt can be deleted");

            grn.IsDeleted = true;
            grn.DeletedAt = DateTime.UtcNow;
            grn.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        private async Task<Guid> CreateGoodsReceiptJournal(GoodsReceiptEntity grn, Guid userId)
        {
            var description = $"Auto journal for {grn.GoodsReceiptNumber}";
            var inventoryByAccount = new Dictionary<Guid, decimal>();

            foreach (var line in grn.Lines)
            {
                var accountId = await GetItemInventoryAccountIdAsync(line.ItemId);
                var amount = line.Quantity * line.UnitPrice;
                if (!inventoryByAccount.ContainsKey(accountId)) inventoryByAccount[accountId] = 0;
                inventoryByAccount[accountId] += amount;
            }

            var journalLines = inventoryByAccount.Select(x => new JournalLineRequest
            {
                AccountId = x.Key,
                Description = description,
                DebitAmount = x.Value,
                CreditAmount = 0
            }).ToList();

            journalLines.Add(new JournalLineRequest
            {
                AccountId = AccountsPayableAccountId,
                Description = description,
                DebitAmount = 0,
                CreditAmount = grn.SubTotal
            });

            var journalId = await _journalEntryService.CreateAsync(new CreateJournalEntryRequest
            {
                JournalDate = grn.ReceiptDate,
                Description = description,
                JournalLines = journalLines
            }, userId);

            await _journalEntryService.PostAsync(new PostJournalRequest
            {
                JournalId = journalId,
                PostedDate = grn.ReceiptDate
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

        private async Task<Dictionary<Guid, decimal>> GetReceivedByPoLineAsync()
        {
            return await _dbContext.GoodsReceiptLines
                .Where(x => !x.IsDeleted && x.GoodsReceipt != null && !x.GoodsReceipt.IsDeleted && x.GoodsReceipt.Status == "Posted")
                .GroupBy(x => x.PurchaseOrderLineId)
                .Select(g => new { PurchaseOrderLineId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.PurchaseOrderLineId, x => x.Quantity);
        }

        private async Task<string> GenerateNumberAsync()
        {
            var last = await _dbContext.GoodsReceipts.Where(x => x.GoodsReceiptNumber.StartsWith("GRN-"))
                .OrderByDescending(x => x.GoodsReceiptNumber).Select(x => x.GoodsReceiptNumber).FirstOrDefaultAsync();
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[4..]) + 1;
            return $"GRN-{next:D5}";
        }
    }
}
