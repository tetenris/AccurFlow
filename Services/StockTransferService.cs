using AccuFlow.Infrastructure.Persistence;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.StockTransfer;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface IStockTransferService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableStockTransferRequest request);
        Task<StockTransferDetailViewModel?> GetById(Guid id);
        Task<List<WarehouseEntity>> GetWarehouses();
        Task Create(CreateStockTransferRequest request, Guid userId);
        Task Update(UpdateStockTransferRequest request, Guid userId);
        Task Post(Guid id, Guid userId);
        Task Delete(Guid id, Guid userId);
    }

    public class StockTransferService : BaseService, IStockTransferService
    {
        public StockTransferService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<BaseDatatableResponse> Datatable(DataTableStockTransferRequest request)
        {
            var query = _dbContext.StockTransfers
                .Include(x => x.FromWarehouse).Include(x => x.ToWarehouse)
                .Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            if (request.WarehouseId.HasValue) query = query.Where(x => x.FromWarehouseId == request.WarehouseId || x.ToWarehouseId == request.WarehouseId);
            if (request.DateFrom.HasValue) query = query.Where(x => x.TransferDate >= request.DateFrom.Value.Date);
            if (request.DateTo.HasValue) query = query.Where(x => x.TransferDate <= request.DateTo.Value.Date);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.StockTransferNumber.ToLower().Contains(search));
            }
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.TransferDate).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new StockTransferViewModel
                {
                    StockTransferId = x.StockTransferId,
                    StockTransferNumber = x.StockTransferNumber,
                    TransferDate = x.TransferDate,
                    FromWarehouseName = x.FromWarehouse.WarehouseName,
                    ToWarehouseName = x.ToWarehouse.WarehouseName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalQuantity = x.Lines.Sum(l => l.Quantity)
                }).ToListAsync();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<StockTransferDetailViewModel?> GetById(Guid id)
        {
            return await _dbContext.StockTransfers
                .Include(x => x.FromWarehouse).Include(x => x.ToWarehouse)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.StockTransferId == id && !x.IsDeleted)
                .Select(x => new StockTransferDetailViewModel
                {
                    StockTransferId = x.StockTransferId,
                    StockTransferNumber = x.StockTransferNumber,
                    TransferDate = x.TransferDate,
                    FromWarehouseId = x.FromWarehouseId,
                    ToWarehouseId = x.ToWarehouseId,
                    FromWarehouseName = x.FromWarehouse.WarehouseName,
                    ToWarehouseName = x.ToWarehouse.WarehouseName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalQuantity = x.Lines.Sum(l => l.Quantity),
                    Notes = x.Notes,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanPost = x.Status == "Draft",
                    Lines = x.Lines.Where(l => !l.IsDeleted).Select(l => new StockTransferLineViewModel
                    {
                        StockTransferLineId = l.StockTransferLineId,
                        ItemId = l.ItemId,
                        ItemCode = l.Item.ItemCode,
                        ItemName = l.Item.ItemName,
                        Quantity = l.Quantity
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

        public async Task Create(CreateStockTransferRequest request, Guid userId)
        {
            if (request.FromWarehouseId == Guid.Empty) throw new Exception("From warehouse is required");
            if (request.ToWarehouseId == Guid.Empty) throw new Exception("To warehouse is required");
            if (request.FromWarehouseId == request.ToWarehouseId) throw new Exception("From and to warehouse must be different");
            if (request.Lines == null || request.Lines.Count == 0) throw new Exception("At least one item line is required");

            var transfer = new StockTransferEntity
            {
                StockTransferId = Guid.NewGuid(),
                StockTransferNumber = await GenerateNumberAsync(),
                TransferDate = request.TransferDate,
                FromWarehouseId = request.FromWarehouseId,
                ToWarehouseId = request.ToWarehouseId,
                Status = "Draft",
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            };
            foreach (var line in request.Lines)
            {
                if (line.Quantity <= 0) continue;
                transfer.Lines.Add(new StockTransferLineEntity
                {
                    StockTransferLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Quantity = line.Quantity,
                    CreatedBy = userId.ToString()
                });
            }
            if (transfer.Lines.Count == 0) throw new Exception("At least one item line with quantity above zero is required");
            _dbContext.StockTransfers.Add(transfer);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(UpdateStockTransferRequest request, Guid userId)
        {
            var transfer = await _dbContext.StockTransfers.Include(x => x.Lines).FirstOrDefaultAsync(x => x.StockTransferId == request.StockTransferId && !x.IsDeleted);
            if (transfer == null) throw new Exception("Stock transfer not found");
            if (transfer.Status != "Draft") throw new Exception("Only draft stock transfer can be edited");
            if (request.FromWarehouseId == request.ToWarehouseId) throw new Exception("From and to warehouse must be different");

            transfer.TransferDate = request.TransferDate;
            transfer.FromWarehouseId = request.FromWarehouseId;
            transfer.ToWarehouseId = request.ToWarehouseId;
            transfer.Notes = request.Notes;
            transfer.UpdatedAt = DateTime.UtcNow;
            transfer.UpdatedBy = userId.ToString();
            foreach (var existing in transfer.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true; existing.DeletedAt = DateTime.UtcNow; existing.DeletedBy = userId.ToString();
            }
            foreach (var line in request.Lines)
            {
                if (line.Quantity <= 0) continue;
                transfer.Lines.Add(new StockTransferLineEntity
                {
                    StockTransferLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Quantity = line.Quantity,
                    CreatedBy = userId.ToString()
                });
            }
            if (!transfer.Lines.Any(l => !l.IsDeleted)) throw new Exception("At least one item line is required");
            await _dbContext.SaveChangesAsync();
        }

        public async Task Post(Guid id, Guid userId)
        {
            var transfer = await _dbContext.StockTransfers
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.StockTransferId == id && !x.IsDeleted);
            if (transfer == null) throw new Exception("Stock transfer not found");
            if (transfer.Status == "Posted") throw new Exception("Stock transfer is already posted");
            if (transfer.Status != "Draft") throw new Exception("Only draft stock transfer can be posted");

            foreach (var line in transfer.Lines)
            {
                var available = await GetAvailableStockAsync(line.ItemId, transfer.FromWarehouseId);
                if (available < line.Quantity)
                    throw new Exception($"Insufficient stock for {GetItemLabel(line.ItemId)} in source warehouse (available: {available}, transfer: {line.Quantity})");
                var unitCost = await _dbContext.Items.Where(x => x.ItemId == line.ItemId).Select(x => x.PurchasePrice).FirstOrDefaultAsync();
                _dbContext.StockMovements.Add(new StockMovementEntity
                {
                    StockMovementId = Guid.NewGuid(),
                    MovementDate = transfer.TransferDate,
                    ItemId = line.ItemId,
                    WarehouseId = transfer.FromWarehouseId,
                    MovementType = "Stock Transfer Out",
                    SourceDocumentType = "StockTransfer",
                    SourceDocumentId = transfer.StockTransferId,
                    QuantityIn = 0,
                    QuantityOut = line.Quantity,
                    UnitCost = unitCost,
                    Notes = $"Transfer {transfer.StockTransferNumber} to warehouse",
                    CreatedBy = userId.ToString()
                });
                _dbContext.StockMovements.Add(new StockMovementEntity
                {
                    StockMovementId = Guid.NewGuid(),
                    MovementDate = transfer.TransferDate,
                    ItemId = line.ItemId,
                    WarehouseId = transfer.ToWarehouseId,
                    MovementType = "Stock Transfer In",
                    SourceDocumentType = "StockTransfer",
                    SourceDocumentId = transfer.StockTransferId,
                    QuantityIn = line.Quantity,
                    QuantityOut = 0,
                    UnitCost = unitCost,
                    Notes = $"Transfer {transfer.StockTransferNumber} from warehouse",
                    CreatedBy = userId.ToString()
                });
            }

            transfer.Status = "Posted";
            transfer.UpdatedAt = DateTime.UtcNow;
            transfer.UpdatedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var transfer = await _dbContext.StockTransfers.FirstOrDefaultAsync(x => x.StockTransferId == id && !x.IsDeleted);
            if (transfer == null) throw new Exception("Stock transfer not found");
            if (transfer.Status != "Draft") throw new Exception("Only draft stock transfer can be deleted");
            transfer.IsDeleted = true; transfer.DeletedAt = DateTime.UtcNow; transfer.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        private async Task<decimal> GetAvailableStockAsync(Guid itemId, Guid warehouseId)
        {
            return await _dbContext.StockMovements
                .Where(x => x.ItemId == itemId && x.WarehouseId == warehouseId && !x.IsDeleted)
                .SumAsync(x => x.QuantityIn - x.QuantityOut);
        }

        private async Task<string> GetItemLabel(Guid itemId)
        {
            var item = await _dbContext.Items.Where(x => x.ItemId == itemId).Select(x => new { x.ItemCode, x.ItemName }).FirstOrDefaultAsync();
            return item == null ? itemId.ToString() : $"{item.ItemCode} - {item.ItemName}";
        }

        private async Task<string> GenerateNumberAsync()
        {
            var last = await _dbContext.StockTransfers.Where(x => x.StockTransferNumber.StartsWith("ST-"))
                .OrderByDescending(x => x.StockTransferNumber).Select(x => x.StockTransferNumber).FirstOrDefaultAsync();
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[3..]) + 1;
            return $"ST-{next:D5}";
        }
    }
}

