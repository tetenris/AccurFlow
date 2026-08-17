using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.StockBatch;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface IStockBatchService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(BaseDatatableRequest request, Guid? itemId = null);
        Task<List<ItemEntity>> GetItems();
        Task Create(StockBatchCreateRequest request, Guid userId);
        Task Consume(StockBatchConsumeRequest request, Guid userId);
        Task Delete(Guid id, Guid userId);
    }

    public class StockBatchService : BaseService, IStockBatchService
    {
        public StockBatchService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<BaseDatatableResponse> Datatable(BaseDatatableRequest request, Guid? itemId = null)
        {
            var query = _dbContext.StockBatches
                .Include(x => x.Item)
                .Where(x => !x.IsDeleted);

            if (itemId.HasValue) query = query.Where(x => x.ItemId == itemId.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.BatchNumber.ToLower().Contains(search)
                    || x.Item.ItemCode.ToLower().Contains(search)
                    || x.Item.ItemName.ToLower().Contains(search));
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new StockBatchViewModel
                {
                    StockBatchId = x.StockBatchId,
                    ItemId = x.ItemId,
                    ItemCode = x.Item.ItemCode,
                    ItemName = x.Item.ItemName,
                    BatchNumber = x.BatchNumber,
                    Quantity = x.Quantity,
                    RemainingQuantity = x.RemainingQuantity,
                    ExpiryDate = x.ExpiryDate,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    Status = x.RemainingQuantity <= 0 ? "Out" : (x.RemainingQuantity < x.Quantity ? "Partial" : "Available")
                }).ToListAsync();

            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<List<ItemEntity>> GetItems()
        {
            return await _dbContext.Items
                .Where(x => x.IsActive && !x.IsDeleted && x.ItemType == "Inventory")
                .OrderBy(x => x.ItemCode)
                .ToListAsync();
        }

        public async Task Create(StockBatchCreateRequest request, Guid userId)
        {
            if (request.ItemId == Guid.Empty) throw new Exception("Item is required");
            if (string.IsNullOrWhiteSpace(request.BatchNumber)) throw new Exception("Batch/serial number is required");
            if (request.Quantity <= 0) throw new Exception("Quantity must be greater than zero");

            var item = await _dbContext.Items.FirstOrDefaultAsync(x => x.ItemId == request.ItemId && !x.IsDeleted);
            if (item == null) throw new Exception("Item not found");

            var batch = new StockBatchEntity
            {
                StockBatchId = Guid.NewGuid(),
                ItemId = request.ItemId,
                BatchNumber = request.BatchNumber.Trim(),
                Quantity = request.Quantity,
                RemainingQuantity = request.Quantity,
                ExpiryDate = request.ExpiryDate,
                Notes = request.Notes,
                IsActive = true,
                CreatedBy = userId.ToString()
            };

            _dbContext.StockBatches.Add(batch);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Consume(StockBatchConsumeRequest request, Guid userId)
        {
            if (request.Quantity <= 0) throw new Exception("Quantity must be greater than zero");

            var batch = await _dbContext.StockBatches
                .FirstOrDefaultAsync(x => x.StockBatchId == request.StockBatchId && !x.IsDeleted && x.IsActive);
            if (batch == null) throw new Exception("Batch not found");

            if (request.Quantity > batch.RemainingQuantity)
                throw new Exception($"Cannot consume more than remaining quantity ({batch.RemainingQuantity})");

            batch.RemainingQuantity -= request.Quantity;
            batch.UpdatedAt = DateTime.UtcNow;
            batch.UpdatedBy = userId.ToString();

            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var batch = await _dbContext.StockBatches.FirstOrDefaultAsync(x => x.StockBatchId == id && !x.IsDeleted);
            if (batch == null) throw new Exception("Batch not found");
            if (batch.RemainingQuantity < batch.Quantity)
                throw new Exception("Cannot delete batch that has been partially consumed");

            batch.IsDeleted = true;
            batch.DeletedAt = DateTime.UtcNow;
            batch.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }
    }
}