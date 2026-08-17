using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.StockOpname;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface IStockOpnameService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableStockOpnameRequest request);
        Task<StockOpnameDetailViewModel?> GetById(Guid id);
        Task<List<WarehouseViewModel>> GetWarehouses();
        Task<List<StockOpnameLineViewModel>> GetStockQuantities(Guid warehouseId);
        Task Create(CreateStockOpnameRequest request, Guid userId);
        Task Update(UpdateStockOpnameRequest request, Guid userId);
        Task Post(Guid id, Guid userId);
        Task Delete(Guid id, Guid userId);
    }

    public class StockOpnameService : BaseService, IStockOpnameService
    {
        public StockOpnameService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<BaseDatatableResponse> Datatable(DataTableStockOpnameRequest request)
        {
            var query = _dbContext.StockOpnames
                .Include(x => x.Warehouse)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.StockOpnameNumber.ToLower().Contains(search) || x.Warehouse.WarehouseName.ToLower().Contains(search));
            }

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.OpnameDate)
                .Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new StockOpnameViewModel
                {
                    StockOpnameId = x.StockOpnameId,
                    StockOpnameNumber = x.StockOpnameNumber,
                    OpnameDate = x.OpnameDate,
                    WarehouseId = x.WarehouseId,
                    WarehouseName = x.Warehouse.WarehouseName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalDifference = x.Lines.Sum(l => l.DifferenceQuantity)
                }).ToListAsync();

            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<StockOpnameDetailViewModel?> GetById(Guid id)
        {
            return await _dbContext.StockOpnames
                .Include(x => x.Warehouse)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.StockOpnameId == id && !x.IsDeleted)
                .Select(x => new StockOpnameDetailViewModel
                {
                    StockOpnameId = x.StockOpnameId,
                    StockOpnameNumber = x.StockOpnameNumber,
                    OpnameDate = x.OpnameDate,
                    WarehouseId = x.WarehouseId,
                    WarehouseName = x.Warehouse.WarehouseName,
                    Status = x.Status,
                    Notes = x.Notes,
                    LineCount = x.Lines.Count,
                    TotalDifference = x.Lines.Sum(l => l.DifferenceQuantity),
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanPost = x.Status == "Draft",
                    Lines = x.Lines.Select(l => new StockOpnameLineViewModel
                    {
                        ItemId = l.ItemId,
                        ItemCode = l.Item.ItemCode,
                        ItemName = l.Item.ItemName,
                        Unit = l.Item.Unit,
                        SystemQuantity = l.SystemQuantity,
                        ActualQuantity = l.ActualQuantity,
                        DifferenceQuantity = l.DifferenceQuantity,
                        UnitCost = l.Item.PurchasePrice,
                        Notes = l.Notes
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task<List<WarehouseViewModel>> GetWarehouses()
        {
            return await _dbContext.Warehouses
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.WarehouseCode)
                .Select(x => new WarehouseViewModel
                {
                    WarehouseId = x.WarehouseId,
                    WarehouseCode = x.WarehouseCode,
                    WarehouseName = x.WarehouseName
                })
                .ToListAsync();
        }

        public async Task<List<StockOpnameLineViewModel>> GetStockQuantities(Guid warehouseId)
        {
            var current = await GetCurrentStockAsync(warehouseId);
            var items = await _dbContext.Items
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.ItemCode)
                .ToListAsync();

            return items
                .Where(i => current.ContainsKey(i.ItemId))
                .Select(i => new StockOpnameLineViewModel
                {
                    ItemId = i.ItemId,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    Unit = i.Unit,
                    SystemQuantity = current[i.ItemId],
                    ActualQuantity = current[i.ItemId],
                    DifferenceQuantity = 0,
                    UnitCost = i.PurchasePrice
                })
                .ToList();
        }

        public async Task Create(CreateStockOpnameRequest request, Guid userId)
        {
            if (request.WarehouseId == Guid.Empty) throw new Exception("Warehouse is required");
            if (request.Lines == null || request.Lines.Count == 0) throw new Exception("At least one item line is required");

            var warehouse = await _dbContext.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == request.WarehouseId && !x.IsDeleted);
            if (warehouse == null) throw new Exception("Warehouse not found");

            var opname = new StockOpnameEntity
            {
                StockOpnameId = Guid.NewGuid(),
                StockOpnameNumber = await GenerateNumberAsync(),
                OpnameDate = request.OpnameDate,
                WarehouseId = request.WarehouseId,
                Status = "Draft",
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            };

            foreach (var line in request.Lines)
            {
                opname.Lines.Add(new StockOpnameLineEntity
                {
                    StockOpnameLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    SystemQuantity = line.SystemQuantity,
                    ActualQuantity = line.ActualQuantity,
                    DifferenceQuantity = line.ActualQuantity - line.SystemQuantity,
                    Notes = line.Notes,
                    CreatedBy = userId.ToString()
                });
            }

            _dbContext.StockOpnames.Add(opname);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(UpdateStockOpnameRequest request, Guid userId)
        {
            var opname = await _dbContext.StockOpnames
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.StockOpnameId == request.StockOpnameId && !x.IsDeleted);
            if (opname == null) throw new Exception("Stock opname not found");
            if (opname.Status != "Draft") throw new Exception("Only draft stock opname can be edited");

            opname.WarehouseId = request.WarehouseId;
            opname.OpnameDate = request.OpnameDate;
            opname.Notes = request.Notes;
            opname.UpdatedAt = DateTime.UtcNow;
            opname.UpdatedBy = userId.ToString();

            foreach (var existing in opname.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true;
                existing.DeletedAt = DateTime.UtcNow;
                existing.DeletedBy = userId.ToString();
            }

            foreach (var line in request.Lines)
            {
                opname.Lines.Add(new StockOpnameLineEntity
                {
                    StockOpnameLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    SystemQuantity = line.SystemQuantity,
                    ActualQuantity = line.ActualQuantity,
                    DifferenceQuantity = line.ActualQuantity - line.SystemQuantity,
                    Notes = line.Notes,
                    CreatedBy = userId.ToString()
                });
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task Post(Guid id, Guid userId)
        {
            var opname = await _dbContext.StockOpnames
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.StockOpnameId == id && !x.IsDeleted);
            if (opname == null) throw new Exception("Stock opname not found");
            if (opname.Status == "Posted") throw new Exception("Stock opname is already posted");
            if (opname.Status != "Draft") throw new Exception("Only draft stock opname can be posted");

            var adjustments = opname.Lines.Where(l => l.DifferenceQuantity != 0).ToList();
            foreach (var line in adjustments)
            {
                var unitCost = await _dbContext.Items
                    .Where(x => x.ItemId == line.ItemId && !x.IsDeleted)
                    .Select(x => x.PurchasePrice)
                    .FirstOrDefaultAsync();

                var difference = line.DifferenceQuantity;
                _dbContext.StockMovements.Add(new StockMovementEntity
                {
                    StockMovementId = Guid.NewGuid(),
                    MovementDate = opname.OpnameDate,
                    ItemId = line.ItemId,
                    WarehouseId = opname.WarehouseId,
                    MovementType = "Opname Adjustment",
                    SourceDocumentType = "StockOpname",
                    SourceDocumentId = opname.StockOpnameId,
                    QuantityIn = difference > 0 ? difference : 0,
                    QuantityOut = difference < 0 ? -difference : 0,
                    UnitCost = unitCost,
                    Notes = string.IsNullOrWhiteSpace(line.Notes)
                        ? $"Stock opname {opname.StockOpnameNumber}"
                        : line.Notes,
                    CreatedBy = userId.ToString()
                });
            }

            opname.Status = "Posted";
            opname.UpdatedAt = DateTime.UtcNow;
            opname.UpdatedBy = userId.ToString();

            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var opname = await _dbContext.StockOpnames.FirstOrDefaultAsync(x => x.StockOpnameId == id && !x.IsDeleted);
            if (opname == null) throw new Exception("Stock opname not found");
            if (opname.Status != "Draft") throw new Exception("Only draft stock opname can be deleted");

            opname.IsDeleted = true;
            opname.DeletedAt = DateTime.UtcNow;
            opname.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        private async Task<string> GenerateNumberAsync()
        {
            var last = await _dbContext.StockOpnames.Where(x => x.StockOpnameNumber.StartsWith("OPN-"))
                .OrderByDescending(x => x.StockOpnameNumber).Select(x => x.StockOpnameNumber).FirstOrDefaultAsync();
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[4..]) + 1;
            return $"OPN-{next:D5}";
        }

        private async Task<Dictionary<Guid, decimal>> GetCurrentStockAsync(Guid warehouseId)
        {
            return await _dbContext.StockMovements
                .Where(x => x.WarehouseId == warehouseId && !x.IsDeleted)
                .GroupBy(x => x.ItemId)
                .Select(g => new { ItemId = g.Key, Quantity = g.Sum(m => m.QuantityIn - m.QuantityOut) })
                .ToDictionaryAsync(x => x.ItemId, x => x.Quantity);
        }
    }
}