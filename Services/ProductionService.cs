using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.JournalEntry;
using AccuFlow.Models.Production;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface IProductionService : IBaseService
    {
        Task<BaseDatatableResponse> BomDatatable(BaseDatatableRequest request);
        Task<BomDetailViewModel?> GetBomById(Guid id);
        Task<List<ItemEntity>> GetItems();
        Task<List<WarehouseEntity>> GetWarehouses();
        Task CreateBom(BomRequest request, Guid userId);
        Task DeleteBom(Guid id, Guid userId);
        Task<List<BomViewModel>> GetBoms();
        Task<BaseDatatableResponse> ProductionOrderDatatable(BaseDatatableRequest request);
        Task CreateProductionOrder(CreateProductionOrderRequest request, Guid userId);
        Task<ProductionOrderDetailViewModel?> GetProductionOrderById(Guid id);
        Task PostProductionOrder(Guid id, Guid userId);
        Task DeleteProductionOrder(Guid id, Guid userId);
    }

    public class ProductionService : BaseService, IProductionService
    {
        private readonly IJournalEntryService _journalEntryService;

        public ProductionService(AppDbContext dbContext, IJournalEntryService journalEntryService) : base(dbContext)
        {
            _journalEntryService = journalEntryService;
        }

        public async Task<BaseDatatableResponse> BomDatatable(BaseDatatableRequest request)
        {
            var query = _dbContext.BillOfMaterials
                .Include(x => x.FinishedItem)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.BomNumber.ToLower().Contains(search)
                    || x.FinishedItem.ItemCode.ToLower().Contains(search)
                    || x.FinishedItem.ItemName.ToLower().Contains(search));
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new BomViewModel
                {
                    BomId = x.BomId,
                    BomNumber = x.BomNumber,
                    FinishedItemId = x.FinishedItemId,
                    FinishedItemCode = x.FinishedItem.ItemCode,
                    FinishedItemName = x.FinishedItem.ItemName,
                    LineCount = x.Lines.Count,
                    IsActive = x.IsActive,
                    Notes = x.Notes
                }).ToListAsync();

            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<BomDetailViewModel?> GetBomById(Guid id)
        {
            return await _dbContext.BillOfMaterials
                .Include(x => x.FinishedItem)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(l => l.ComponentItem)
                .Where(x => x.BomId == id && !x.IsDeleted)
                .Select(x => new BomDetailViewModel
                {
                    BomId = x.BomId,
                    BomNumber = x.BomNumber,
                    FinishedItemId = x.FinishedItemId,
                    FinishedItemCode = x.FinishedItem.ItemCode,
                    FinishedItemName = x.FinishedItem.ItemName,
                    IsActive = x.IsActive,
                    Notes = x.Notes,
                    Lines = x.Lines.OrderBy(l => l.ComponentItem.ItemCode).Select(l => new BomLineViewModel
                    {
                        BomLineId = l.BomLineId,
                        ComponentItemId = l.ComponentItemId,
                        ComponentItemCode = l.ComponentItem.ItemCode,
                        ComponentItemName = l.ComponentItem.ItemName,
                        QuantityPerUnit = l.QuantityPerUnit
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task<List<ItemEntity>> GetItems()
        {
            return await _dbContext.Items
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.ItemCode)
                .ToListAsync();
        }

        public async Task<List<WarehouseEntity>> GetWarehouses()
        {
            return await _dbContext.Warehouses
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.WarehouseCode)
                .ToListAsync();
        }

        public async Task CreateBom(BomRequest request, Guid userId)
        {
            if (request.FinishedItemId == Guid.Empty) throw new Exception("Finished item is required");
            if (request.Lines == null || request.Lines.Count == 0) throw new Exception("At least one component line is required");
            if (request.Lines.Any(l => l.QuantityPerUnit <= 0)) throw new Exception("Component quantity must be greater than zero");
            if (request.Lines.Any(l => l.ComponentItemId == request.FinishedItemId)) throw new Exception("Component cannot be the same as finished item");

            var bom = new BillOfMaterialEntity
            {
                BomId = Guid.NewGuid(),
                BomNumber = await GenerateBomNumberAsync(),
                FinishedItemId = request.FinishedItemId,
                Notes = request.Notes,
                IsActive = true,
                CreatedBy = userId.ToString()
            };

            foreach (var line in request.Lines)
            {
                bom.Lines.Add(new BillOfMaterialLineEntity
                {
                    BomLineId = Guid.NewGuid(),
                    ComponentItemId = line.ComponentItemId,
                    QuantityPerUnit = line.QuantityPerUnit,
                    CreatedBy = userId.ToString()
                });
            }

            _dbContext.BillOfMaterials.Add(bom);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteBom(Guid id, Guid userId)
        {
            var bom = await _dbContext.BillOfMaterials.FirstOrDefaultAsync(x => x.BomId == id && !x.IsDeleted);
            if (bom == null) throw new Exception("Bill of material not found");

            var usedInOrder = await _dbContext.ProductionOrders.AnyAsync(x => x.BomId == id && !x.IsDeleted && x.Status != "Draft");
            if (usedInOrder) throw new Exception("Cannot delete BOM that already has posted production orders");

            bom.IsDeleted = true;
            bom.DeletedAt = DateTime.UtcNow;
            bom.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<BomViewModel>> GetBoms()
        {
            return await _dbContext.BillOfMaterials
                .Include(x => x.FinishedItem)
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new BomViewModel
                {
                    BomId = x.BomId,
                    BomNumber = x.BomNumber,
                    FinishedItemId = x.FinishedItemId,
                    FinishedItemCode = x.FinishedItem.ItemCode,
                    FinishedItemName = x.FinishedItem.ItemName,
                    LineCount = x.Lines.Count,
                    IsActive = x.IsActive
                }).ToListAsync();
        }

        public async Task<BaseDatatableResponse> ProductionOrderDatatable(BaseDatatableRequest request)
        {
            var query = _dbContext.ProductionOrders
                .Include(x => x.FinishedItem)
                .Include(x => x.Warehouse)
                .Include(x => x.Bom)
                .Include(x => x.JournalEntry)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.ProductionOrderNumber.ToLower().Contains(search)
                    || x.FinishedItem.ItemCode.ToLower().Contains(search)
                    || x.FinishedItem.ItemName.ToLower().Contains(search));
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(x => x.ProductionDate)
                .Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new ProductionOrderViewModel
                {
                    ProductionOrderId = x.ProductionOrderId,
                    ProductionOrderNumber = x.ProductionOrderNumber,
                    BomNumber = x.Bom != null ? x.Bom.BomNumber : null,
                    FinishedItemCode = x.FinishedItem.ItemCode,
                    FinishedItemName = x.FinishedItem.ItemName,
                    Quantity = x.Quantity,
                    ProductionDate = x.ProductionDate,
                    WarehouseName = x.Warehouse.WarehouseName,
                    Status = x.Status,
                    JournalNumber = x.JournalEntry != null ? x.JournalEntry.JournalNumber : null,
                    Notes = x.Notes,
                    LineCount = x.Lines.Count
                }).ToListAsync();

            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task CreateProductionOrder(CreateProductionOrderRequest request, Guid userId)
        {
            if (request.FinishedItemId == Guid.Empty) throw new Exception("Finished item is required");
            if (request.WarehouseId == Guid.Empty) throw new Exception("Warehouse is required");
            if (request.Quantity <= 0) throw new Exception("Production quantity must be greater than zero");

            var item = await _dbContext.Items.FirstOrDefaultAsync(x => x.ItemId == request.FinishedItemId && !x.IsDeleted);
            if (item == null) throw new Exception("Finished item not found");

            var lines = new List<ProductionOrderLineEntity>();

            if (request.BomId.HasValue)
            {
                var bom = await _dbContext.BillOfMaterials
                    .Include(x => x.Lines.Where(l => !l.IsDeleted))
                    .FirstOrDefaultAsync(x => x.BomId == request.BomId.Value && !x.IsDeleted && x.IsActive);
                if (bom == null) throw new Exception("Bill of material not found");
                if (bom.FinishedItemId != request.FinishedItemId) throw new Exception("BOM does not match the finished item");

                foreach (var line in bom.Lines)
                {
                    lines.Add(new ProductionOrderLineEntity
                    {
                        ProductionOrderLineId = Guid.NewGuid(),
                        ComponentItemId = line.ComponentItemId,
                        QuantityRequired = line.QuantityPerUnit * request.Quantity,
                        UnitCost = 0,
                        CreatedBy = userId.ToString()
                    });
                }
            }
            else
            {
                throw new Exception("Select a bill of material to produce from");
            }

            var order = new ProductionOrderEntity
            {
                ProductionOrderId = Guid.NewGuid(),
                ProductionOrderNumber = await GenerateProductionOrderNumberAsync(),
                BomId = request.BomId,
                FinishedItemId = request.FinishedItemId,
                Quantity = request.Quantity,
                ProductionDate = request.ProductionDate,
                WarehouseId = request.WarehouseId,
                Status = "Draft",
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            };
            order.Lines = lines;

            _dbContext.ProductionOrders.Add(order);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<ProductionOrderDetailViewModel?> GetProductionOrderById(Guid id)
        {
            return await _dbContext.ProductionOrders
                .Include(x => x.FinishedItem)
                .Include(x => x.Warehouse)
                .Include(x => x.Bom)
                .Include(x => x.JournalEntry)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(l => l.ComponentItem)
                .Where(x => x.ProductionOrderId == id && !x.IsDeleted)
                .Select(x => new ProductionOrderDetailViewModel
                {
                    ProductionOrderId = x.ProductionOrderId,
                    ProductionOrderNumber = x.ProductionOrderNumber,
                    BomNumber = x.Bom != null ? x.Bom.BomNumber : null,
                    FinishedItemCode = x.FinishedItem.ItemCode,
                    FinishedItemName = x.FinishedItem.ItemName,
                    Quantity = x.Quantity,
                    ProductionDate = x.ProductionDate,
                    WarehouseName = x.Warehouse.WarehouseName,
                    Status = x.Status,
                    JournalNumber = x.JournalEntry != null ? x.JournalEntry.JournalNumber : null,
                    Lines = x.Lines.OrderBy(l => l.ComponentItem.ItemCode).Select(l => new ProductionOrderLineViewModel
                    {
                        ProductionOrderLineId = l.ProductionOrderLineId,
                        ComponentItemId = l.ComponentItemId,
                        ComponentItemCode = l.ComponentItem.ItemCode,
                        ComponentItemName = l.ComponentItem.ItemName,
                        QuantityRequired = l.QuantityRequired,
                        UnitCost = l.UnitCost
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task PostProductionOrder(Guid id, Guid userId)
        {
            var order = await _dbContext.ProductionOrders
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.ProductionOrderId == id && !x.IsDeleted);
            if (order == null) throw new Exception("Production order not found");
            if (order.Status != "Draft") throw new Exception("Only draft production order can be posted");
            if (order.Lines.Count == 0) throw new Exception("Production order has no component lines");

            var onHand = await GetCurrentStockAsync(order.WarehouseId);
            var finishedItem = await _dbContext.Items.FirstOrDefaultAsync(x => x.ItemId == order.FinishedItemId && !x.IsDeleted);
            if (finishedItem == null) throw new Exception("Finished item not found");

            foreach (var line in order.Lines)
            {
                var component = await _dbContext.Items.FirstOrDefaultAsync(x => x.ItemId == line.ComponentItemId && !x.IsDeleted);
                if (component == null) throw new Exception("Component item not found");
                var available = onHand.GetValueOrDefault(line.ComponentItemId, 0);
                if (line.QuantityRequired > available)
                    throw new Exception($"Insufficient stock for {component.ItemCode} ({component.ItemName}). Required: {line.QuantityRequired}, available: {available}");
                line.UnitCost = component.PurchasePrice;
            }

            var componentCostByAccount = new Dictionary<Guid, decimal>();
            foreach (var line in order.Lines)
            {
                if (line.UnitCost == 0 && line.QuantityRequired > 0)
                    throw new Exception($"Component cost required for quantity {line.QuantityRequired}. Set Purchase Price on the component item.");

                _dbContext.StockMovements.Add(new StockMovementEntity
                {
                    StockMovementId = Guid.NewGuid(),
                    MovementDate = order.ProductionDate,
                    ItemId = line.ComponentItemId,
                    WarehouseId = order.WarehouseId,
                    MovementType = "Production Consumption",
                    SourceDocumentType = "ProductionOrder",
                    SourceDocumentId = order.ProductionOrderId,
                    QuantityIn = 0,
                    QuantityOut = line.QuantityRequired,
                    UnitCost = line.UnitCost,
                    Notes = $"Production {order.ProductionOrderNumber}",
                    CreatedBy = userId.ToString()
                });

                var lineCost = line.QuantityRequired * line.UnitCost;
                var accountId = await GetItemInventoryAccountIdAsync(line.ComponentItemId);
                if (!componentCostByAccount.ContainsKey(accountId)) componentCostByAccount[accountId] = 0;
                componentCostByAccount[accountId] += lineCost;
            }

            _dbContext.StockMovements.Add(new StockMovementEntity
            {
                StockMovementId = Guid.NewGuid(),
                MovementDate = order.ProductionDate,
                ItemId = order.FinishedItemId,
                WarehouseId = order.WarehouseId,
                MovementType = "Production Output",
                SourceDocumentType = "ProductionOrder",
                SourceDocumentId = order.ProductionOrderId,
                QuantityIn = order.Quantity,
                QuantityOut = 0,
                UnitCost = componentCostByAccount.Values.Sum(),
                Notes = $"Production {order.ProductionOrderNumber}",
                CreatedBy = userId.ToString()
            });

            var journalId = await CreateProductionJournal(order, componentCostByAccount, userId);
            order.JournalId = journalId;
            order.Status = "Posted";
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = userId.ToString();

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteProductionOrder(Guid id, Guid userId)
        {
            var order = await _dbContext.ProductionOrders.FirstOrDefaultAsync(x => x.ProductionOrderId == id && !x.IsDeleted);
            if (order == null) throw new Exception("Production order not found");
            if (order.Status != "Draft") throw new Exception("Only draft production order can be deleted");

            order.IsDeleted = true;
            order.DeletedAt = DateTime.UtcNow;
            order.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        private async Task<Guid> CreateProductionJournal(ProductionOrderEntity order, Dictionary<Guid, decimal> componentCostByAccount, Guid userId)
        {
            var description = $"Auto journal for production {order.ProductionOrderNumber}";
            var lines = new List<JournalLineRequest>();
            foreach (var entry in componentCostByAccount)
            {
                lines.Add(new JournalLineRequest
                {
                    AccountId = entry.Key,
                    Description = description,
                    DebitAmount = 0,
                    CreditAmount = entry.Value
                });
            }

            var finishedAccountId = await GetItemInventoryAccountIdAsync(order.FinishedItemId);
            var totalCost = componentCostByAccount.Values.Sum();
            lines.Insert(0, new JournalLineRequest
            {
                AccountId = finishedAccountId,
                Description = description,
                DebitAmount = totalCost,
                CreditAmount = 0
            });

            var journalId = await _journalEntryService.CreateAsync(new CreateJournalEntryRequest
            {
                JournalDate = order.ProductionDate,
                Description = description,
                JournalLines = lines
            }, userId);

            await _journalEntryService.PostAsync(new PostJournalRequest { JournalId = journalId, PostedDate = order.ProductionDate }, userId);
            return journalId;
        }

        private async Task<Guid> GetItemInventoryAccountIdAsync(Guid itemId)
        {
            var accountId = await _dbContext.Items
                .Where(x => x.ItemId == itemId && !x.IsDeleted)
                .Select(x => x.InventoryAccountId)
                .FirstOrDefaultAsync();
            return accountId ?? Guid.Parse("10000000-0000-0000-0000-000000000005");
        }

        private async Task<Dictionary<Guid, decimal>> GetCurrentStockAsync(Guid warehouseId)
        {
            return await _dbContext.StockMovements
                .Where(x => x.WarehouseId == warehouseId && !x.IsDeleted)
                .GroupBy(x => x.ItemId)
                .Select(g => new { ItemId = g.Key, Quantity = g.Sum(m => m.QuantityIn - m.QuantityOut) })
                .ToDictionaryAsync(x => x.ItemId, x => x.Quantity);
        }

        private async Task<string> GenerateBomNumberAsync()
        {
            var last = await _dbContext.BillOfMaterials.Where(x => x.BomNumber.StartsWith("BOM-"))
                .OrderByDescending(x => x.BomNumber).Select(x => x.BomNumber).FirstOrDefaultAsync();
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[4..]) + 1;
            return $"BOM-{next:D5}";
        }

        private async Task<string> GenerateProductionOrderNumberAsync()
        {
            var last = await _dbContext.ProductionOrders.Where(x => x.ProductionOrderNumber.StartsWith("PRD-"))
                .OrderByDescending(x => x.ProductionOrderNumber).Select(x => x.ProductionOrderNumber).FirstOrDefaultAsync();
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[4..]) + 1;
            return $"PRD-{next:D5}";
        }
    }
}