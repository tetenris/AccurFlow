using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using AccuFlow.Models.AgingReport;
using AccuFlow.Models.Approval;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.DocumentAttachment;
using AccuFlow.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface IInventoryService : IBaseService
    {
        Task<BaseDatatableResponse> DatatableItems(DataTableItemRequest request);
        Task CreateItem(CreateItemRequest request, Guid userId);
        Task<BaseDatatableResponse> StockCard(DataTableStockMovementRequest request);
    }

    public class InventoryService : BaseService, IInventoryService
    {
        public InventoryService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<BaseDatatableResponse> DatatableItems(DataTableItemRequest request)
        {
            var query = _dbContext.Items.Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.ItemType)) query = query.Where(x => x.ItemType == request.ItemType);
            if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.ItemCode.ToLower().Contains(search) || x.ItemName.ToLower().Contains(search));
            }
            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.ItemCode).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new ItemViewModel
                {
                    ItemId = x.ItemId,
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName,
                    ItemType = x.ItemType,
                    Unit = x.Unit,
                    SalesPrice = x.SalesPrice,
                    PurchasePrice = x.PurchasePrice,
                    IsActive = x.IsActive
                }).ToListAsync();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task CreateItem(CreateItemRequest request, Guid userId)
        {
            var exists = await _dbContext.Items.AnyAsync(x => x.ItemCode == request.ItemCode && !x.IsDeleted);
            if (exists) throw new Exception("Item code already exists");
            _dbContext.Items.Add(new ItemEntity
            {
                ItemId = Guid.NewGuid(),
                ItemCode = request.ItemCode,
                ItemName = request.ItemName,
                ItemType = request.ItemType,
                Unit = request.Unit,
                SalesPrice = request.SalesPrice,
                PurchasePrice = request.PurchasePrice,
                InventoryAccountId = request.InventoryAccountId,
                SalesAccountId = request.SalesAccountId,
                CostOfGoodsSoldAccountId = request.CostOfGoodsSoldAccountId,
                Description = request.Description,
                IsActive = true,
                CreatedBy = userId.ToString()
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task<BaseDatatableResponse> StockCard(DataTableStockMovementRequest request)
        {
            var query = _dbContext.StockMovements.Include(x => x.Item).Include(x => x.Warehouse).Where(x => !x.IsDeleted);
            if (request.ItemId.HasValue) query = query.Where(x => x.ItemId == request.ItemId);
            if (request.WarehouseId.HasValue) query = query.Where(x => x.WarehouseId == request.WarehouseId);
            if (request.DateFrom.HasValue) query = query.Where(x => x.MovementDate >= request.DateFrom.Value.Date);
            if (request.DateTo.HasValue) query = query.Where(x => x.MovementDate <= request.DateTo.Value.Date);
            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.MovementDate).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new StockMovementViewModel
                {
                    StockMovementId = x.StockMovementId,
                    MovementDate = x.MovementDate,
                    ItemName = x.Item.ItemName,
                    WarehouseName = x.Warehouse.WarehouseName,
                    MovementType = x.MovementType,
                    QuantityIn = x.QuantityIn,
                    QuantityOut = x.QuantityOut,
                    UnitCost = x.UnitCost
                }).ToListAsync();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }

    public interface IAgingReportService : IBaseService
    {
        Task<List<AgingReportViewModel>> GetAging(AgingReportRequest request);
    }

    public class AgingReportService : BaseService, IAgingReportService
    {
        public AgingReportService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<List<AgingReportViewModel>> GetAging(AgingReportRequest request)
        {
            var query = _dbContext.Invoices.Include(x => x.Customer).Include(x => x.Supplier)
                .Where(x => !x.IsDeleted && x.Status != "Cancelled" && x.TotalAmount > x.PaidAmount);
            query = request.AgingType == "AP" ? query.Where(x => x.InvoiceType == "Purchase") : query.Where(x => x.InvoiceType == "Sales");
            var invoices = await query.ToListAsync();
            return invoices.GroupBy(x => new
            {
                Code = x.Customer != null ? x.Customer.CustomerCode : x.Supplier != null ? x.Supplier.SupplierCode : string.Empty,
                Name = x.Customer != null ? x.Customer.CustomerName : x.Supplier != null ? x.Supplier.SupplierName : string.Empty
            }).Select(g =>
            {
                var row = new AgingReportViewModel { PartnerCode = g.Key.Code, PartnerName = g.Key.Name };
                foreach (var invoice in g)
                {
                    var outstanding = invoice.TotalAmount - invoice.PaidAmount;
                    var age = (request.AsOfDate.Date - invoice.DueDate.Date).Days;
                    if (age <= 0) row.Current += outstanding;
                    else if (age <= 30) row.Days1To30 += outstanding;
                    else if (age <= 60) row.Days31To60 += outstanding;
                    else if (age <= 90) row.Days61To90 += outstanding;
                    else row.Over90 += outstanding;
                }
                return row;
            }).OrderBy(x => x.PartnerName).ToList();
        }
    }

    public interface IApprovalService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableApprovalRequest request);
        Task Submit(SubmitApprovalRequest request, Guid userId);
        Task Approve(ApprovalActionRequest request, Guid userId);
        Task Reject(ApprovalActionRequest request, Guid userId);
    }

    public class ApprovalService : BaseService, IApprovalService
    {
        public ApprovalService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<BaseDatatableResponse> Datatable(DataTableApprovalRequest request)
        {
            var query = _dbContext.ApprovalRequests.Include(x => x.RequestedByUser).Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.DocumentType)) query = query.Where(x => x.DocumentType == request.DocumentType);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.RequestedAt).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new ApprovalRequestViewModel
                {
                    ApprovalRequestId = x.ApprovalRequestId,
                    DocumentType = x.DocumentType,
                    DocumentId = x.DocumentId,
                    Status = x.Status,
                    RequestedByName = x.RequestedByUser.FullName,
                    RequestedAt = x.RequestedAt,
                    Notes = x.Notes
                }).ToListAsync();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task Submit(SubmitApprovalRequest request, Guid userId)
        {
            _dbContext.ApprovalRequests.Add(new ApprovalRequestEntity
            {
                ApprovalRequestId = Guid.NewGuid(),
                DocumentType = request.DocumentType,
                DocumentId = request.DocumentId,
                Status = "Pending",
                RequestedBy = userId,
                RequestedAt = DateTime.UtcNow,
                CurrentApproverId = request.CurrentApproverId,
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            });
            await _dbContext.SaveChangesAsync();
        }

        public Task Approve(ApprovalActionRequest request, Guid userId) => ChangeStatus(request, userId, "Approved");
        public Task Reject(ApprovalActionRequest request, Guid userId) => ChangeStatus(request, userId, "Rejected");

        private async Task ChangeStatus(ApprovalActionRequest request, Guid userId, string status)
        {
            var approval = await _dbContext.ApprovalRequests.FirstOrDefaultAsync(x => x.ApprovalRequestId == request.ApprovalRequestId && !x.IsDeleted);
            if (approval == null) throw new Exception("Approval request not found");
            approval.Status = status;
            approval.UpdatedBy = userId.ToString();
            _dbContext.ApprovalHistories.Add(new ApprovalHistoryEntity
            {
                ApprovalHistoryId = Guid.NewGuid(),
                ApprovalRequestId = approval.ApprovalRequestId,
                ApproverId = userId,
                Action = status,
                ActionAt = DateTime.UtcNow,
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            });
            await _dbContext.SaveChangesAsync();
        }
    }

    public interface IDocumentAttachmentService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableDocumentAttachmentRequest request);
    }

    public class DocumentAttachmentService : BaseService, IDocumentAttachmentService
    {
        public DocumentAttachmentService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<BaseDatatableResponse> Datatable(DataTableDocumentAttachmentRequest request)
        {
            var query = _dbContext.DocumentAttachments.Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.DocumentType)) query = query.Where(x => x.DocumentType == request.DocumentType);
            if (request.DocumentId.HasValue) query = query.Where(x => x.DocumentId == request.DocumentId);
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.CreatedAt).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new DocumentAttachmentViewModel
                {
                    DocumentAttachmentId = x.DocumentAttachmentId,
                    DocumentType = x.DocumentType,
                    DocumentId = x.DocumentId,
                    FileName = x.FileName,
                    ContentType = x.ContentType,
                    FileSize = x.FileSize,
                    Description = x.Description,
                    CreatedAt = x.CreatedAt
                }).ToListAsync();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}
