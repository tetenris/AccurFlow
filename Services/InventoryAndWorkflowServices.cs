using AccuFlow.Entities.Context;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.AgingReport;
using AccuFlow.Models.Approval;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.DocumentAttachment;
using AccuFlow.Models.Inventory;
using AccuFlow.Models.ReceivablePayable;
using AccuFlow.Models.Tax;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface IInventoryService : IBaseService
    {
        Task<BaseDatatableResponse> DatatableItems(DataTableItemRequest request);
        Task<List<ItemViewModel>> GetActiveItems();
        Task CreateItem(CreateItemRequest request, Guid userId);
        Task<BaseDatatableResponse> StockCard(DataTableStockMovementRequest request);
        Task<BaseDatatableResponse> StockMinimum(DataTableStockMinimumRequest request);
        Task UpdateReorderPoint(UpdateReorderPointRequest request, Guid userId);
    }

    public class InventoryService : BaseService, IInventoryService
    {
        public InventoryService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<BaseDatatableResponse> DatatableItems(DataTableItemRequest request)
        {
            var query = _dbContext.Items.Include(x => x.ItemGroup).Where(x => !x.IsDeleted);
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
                    ItemGroupId = x.ItemGroupId,
                    ItemGroupName = x.ItemGroup != null ? x.ItemGroup.GroupName : null,
                    Unit = x.Unit,
                    SalesPrice = x.SalesPrice,
                    PurchasePrice = x.PurchasePrice,
                    ReorderPoint = x.ReorderPoint,
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
                ItemGroupId = request.ItemGroupId,
                Unit = request.Unit,
                SalesPrice = request.SalesPrice,
                PurchasePrice = request.PurchasePrice,
                ReorderPoint = request.ReorderPoint,
                InventoryAccountId = request.InventoryAccountId,
                SalesAccountId = request.SalesAccountId,
                CostOfGoodsSoldAccountId = request.CostOfGoodsSoldAccountId,
                Description = request.Description,
                IsActive = true,
                CreatedBy = userId.ToString()
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ItemViewModel>> GetActiveItems()
        {
            return await _dbContext.Items
                .Include(x => x.ItemGroup)
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.ItemCode)
                .Select(x => new ItemViewModel
                {
                    ItemId = x.ItemId,
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName,
                    ItemType = x.ItemType,
                    ItemGroupId = x.ItemGroupId,
                    ItemGroupName = x.ItemGroup != null ? x.ItemGroup.GroupName : null,
                    Unit = x.Unit,
                    SalesPrice = x.SalesPrice,
                    PurchasePrice = x.PurchasePrice,
                    IsActive = x.IsActive
                })
                .ToListAsync();
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

        public async Task<BaseDatatableResponse> StockMinimum(DataTableStockMinimumRequest request)
        {
            var items = await _dbContext.Items
                .Where(x => !x.IsDeleted && x.ItemType == "Inventory")
                .Select(x => new { x.ItemId, x.ItemCode, x.ItemName, x.Unit, x.ReorderPoint })
                .ToListAsync();
            var stock = await _dbContext.StockMovements
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.ItemId)
                .Select(g => new { ItemId = g.Key, Quantity = g.Sum(x => x.QuantityIn - x.QuantityOut) })
                .ToListAsync();
            var stockMap = stock.ToDictionary(x => x.ItemId, x => x.Quantity);
            var rows = items.Select(x => new StockMinimumViewModel
            {
                ItemId = x.ItemId,
                ItemCode = x.ItemCode,
                ItemName = x.ItemName,
                Unit = x.Unit,
                ReorderPoint = x.ReorderPoint,
                CurrentStock = stockMap.ContainsKey(x.ItemId) ? stockMap[x.ItemId] : 0,
                IsBelow = (stockMap.ContainsKey(x.ItemId) ? stockMap[x.ItemId] : 0) < x.ReorderPoint
            }).ToList();
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                rows = rows.Where(x => x.ItemCode.ToLower().Contains(search) || x.ItemName.ToLower().Contains(search)).ToList();
            }
            if (request.BelowOnly) rows = rows.Where(x => x.IsBelow).ToList();
            var total = rows.Count;
            var data = rows.OrderByDescending(x => x.IsBelow).ThenBy(x => x.ItemCode).Skip((request.Page - 1) * request.Size).Take(request.Size).ToList();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task UpdateReorderPoint(UpdateReorderPointRequest request, Guid userId)
        {
            var item = await _dbContext.Items.FirstOrDefaultAsync(x => x.ItemId == request.ItemId && !x.IsDeleted);
            if (item == null) throw new Exception("Item not found");
            if (request.ReorderPoint < 0) throw new Exception("Reorder point cannot be negative");
            item.ReorderPoint = request.ReorderPoint;
            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
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

    public interface IReceivablePayableService : IBaseService
    {
        Task<List<ReceivablePayableViewModel>> GetDetail(ReceivablePayableRequest request);
    }

    public class ReceivablePayableService : BaseService, IReceivablePayableService
    {
        public ReceivablePayableService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<List<ReceivablePayableViewModel>> GetDetail(ReceivablePayableRequest request)
        {
            var query = _dbContext.Invoices.Include(x => x.Customer).Include(x => x.Supplier)
                .Where(x => !x.IsDeleted && x.Status != "Cancelled" && x.TotalAmount > x.PaidAmount);
            query = request.ReportType == "AP" ? query.Where(x => x.InvoiceType == "Purchase") : query.Where(x => x.InvoiceType == "Sales");
            var invoices = await query.ToListAsync();
            return invoices.Select(x =>
            {
                var outstanding = x.TotalAmount - x.PaidAmount;
                return new ReceivablePayableViewModel
                {
                    PartnerCode = x.Customer != null ? x.Customer.CustomerCode : x.Supplier != null ? x.Supplier.SupplierCode : string.Empty,
                    PartnerName = x.Customer != null ? x.Customer.CustomerName : x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                    InvoiceNumber = x.InvoiceNumber,
                    InvoiceDate = x.InvoiceDate,
                    DueDate = x.DueDate,
                    TotalAmount = x.TotalAmount,
                    PaidAmount = x.PaidAmount,
                    OutstandingAmount = outstanding,
                    Status = x.DueDate.Date < request.AsOfDate.Date ? "Overdue" : "Open"
                };
            }).OrderBy(x => x.PartnerName).ThenBy(x => x.DueDate).ToList();
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
        Task<List<DocumentAttachmentViewModel>> GetByDocument(string documentType, Guid documentId);
        Task<DocumentAttachmentEntity> Upload(UploadDocumentAttachmentRequest request, IFormFile file, Guid userId);
        Task<DocumentAttachmentEntity> GetFile(Guid id);
        Task Delete(Guid id, Guid userId);
    }

    public class DocumentAttachmentService : BaseService, IDocumentAttachmentService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public DocumentAttachmentService(AppDbContext dbContext, IConfiguration configuration, IWebHostEnvironment environment) : base(dbContext)
        {
            _configuration = configuration;
            _environment = environment;
        }

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

        public async Task<List<DocumentAttachmentViewModel>> GetByDocument(string documentType, Guid documentId)
        {
            return await _dbContext.DocumentAttachments
                .Where(x => x.DocumentType == documentType && x.DocumentId == documentId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
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
                })
                .ToListAsync();
        }

        public async Task<DocumentAttachmentEntity> Upload(UploadDocumentAttachmentRequest request, IFormFile file, Guid userId)
        {
            if (file == null || file.Length == 0) throw new Exception("File is required");

            var maxFileSizeMb = _configuration.GetValue<int?>("Storage:MaxFileSizeMb") ?? 10;
            if (file.Length > maxFileSizeMb * 1024L * 1024L) throw new Exception($"File size must be less than {maxFileSizeMb} MB");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = _configuration.GetSection("Storage:AllowedExtensions").Get<string[]>() ?? Array.Empty<string>();
            if (!allowedExtensions.Contains(extension)) throw new Exception("File extension is not allowed");

            var uploadRoot = GetUploadRoot();
            var documentFolder = Path.Combine(uploadRoot, request.DocumentType, request.DocumentId.ToString());
            Directory.CreateDirectory(documentFolder);

            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(documentFolder, storedFileName);

            await using (var stream = new FileStream(filePath, FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = Path.GetRelativePath(_environment.ContentRootPath, filePath).Replace('\\', '/');
            var attachment = new DocumentAttachmentEntity
            {
                DocumentAttachmentId = Guid.NewGuid(),
                DocumentType = request.DocumentType,
                DocumentId = request.DocumentId,
                FileName = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                FilePath = relativePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                Description = request.Description,
                CreatedBy = userId.ToString()
            };

            _dbContext.DocumentAttachments.Add(attachment);
            await _dbContext.SaveChangesAsync();
            return attachment;
        }

        public async Task<DocumentAttachmentEntity> GetFile(Guid id)
        {
            var attachment = await _dbContext.DocumentAttachments.FirstOrDefaultAsync(x => x.DocumentAttachmentId == id && !x.IsDeleted);
            if (attachment == null) throw new Exception("Attachment not found");
            return attachment;
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var attachment = await _dbContext.DocumentAttachments.FirstOrDefaultAsync(x => x.DocumentAttachmentId == id && !x.IsDeleted);
            if (attachment == null) throw new Exception("Attachment not found");

            var filePath = Path.Combine(_environment.ContentRootPath, attachment.FilePath);
            if (File.Exists(filePath)) File.Delete(filePath);

            attachment.IsDeleted = true;
            attachment.DeletedBy = userId.ToString();
            attachment.DeletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        private string GetUploadRoot()
        {
            var configuredPath = _configuration.GetValue<string>("Storage:UploadsPath") ?? "wwwroot/uploads";
            return Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(_environment.ContentRootPath, configuredPath);
        }
    }

    public interface IItemGroupService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableItemGroupRequest request);
        Task<ItemGroupViewModel?> GetById(Guid id);
        Task<List<ItemGroupViewModel>> GetActiveGroups();
        Task Create(CreateItemGroupRequest request, Guid userId);
        Task Update(UpdateItemGroupRequest request, Guid userId);
        Task Delete(Guid id, Guid userId);
    }

    public class ItemGroupService : BaseService, IItemGroupService
    {
        public ItemGroupService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<BaseDatatableResponse> Datatable(DataTableItemGroupRequest request)
        {
            var query = _dbContext.ItemGroups.Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.GroupCode.ToLower().Contains(search) || x.GroupName.ToLower().Contains(search));
            }
            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.GroupCode).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new ItemGroupViewModel
                {
                    ItemGroupId = x.ItemGroupId,
                    GroupCode = x.GroupCode,
                    GroupName = x.GroupName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    ItemCount = _dbContext.Items.Count(i => i.ItemGroupId == x.ItemGroupId && !i.IsDeleted)
                }).ToListAsync();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<ItemGroupViewModel?> GetById(Guid id)
        {
            return await _dbContext.ItemGroups
                .Where(x => x.ItemGroupId == id && !x.IsDeleted)
                .Select(x => new ItemGroupViewModel
                {
                    ItemGroupId = x.ItemGroupId,
                    GroupCode = x.GroupCode,
                    GroupName = x.GroupName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    ItemCount = _dbContext.Items.Count(i => i.ItemGroupId == x.ItemGroupId && !i.IsDeleted)
                }).FirstOrDefaultAsync();
        }

        public async Task<List<ItemGroupViewModel>> GetActiveGroups()
        {
            return await _dbContext.ItemGroups
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.GroupCode)
                .Select(x => new ItemGroupViewModel
                {
                    ItemGroupId = x.ItemGroupId,
                    GroupCode = x.GroupCode,
                    GroupName = x.GroupName,
                    Description = x.Description,
                    IsActive = x.IsActive
                }).ToListAsync();
        }

        public async Task Create(CreateItemGroupRequest request, Guid userId)
        {
            var exists = await _dbContext.ItemGroups.AnyAsync(x => x.GroupCode == request.GroupCode && !x.IsDeleted);
            if (exists) throw new Exception("Group code already exists");
            _dbContext.ItemGroups.Add(new ItemGroupEntity
            {
                ItemGroupId = Guid.NewGuid(),
                GroupCode = request.GroupCode,
                GroupName = request.GroupName,
                Description = request.Description,
                IsActive = true,
                CreatedBy = userId.ToString()
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(UpdateItemGroupRequest request, Guid userId)
        {
            var group = await _dbContext.ItemGroups.FirstOrDefaultAsync(x => x.ItemGroupId == request.ItemGroupId && !x.IsDeleted);
            if (group == null) throw new Exception("Item group not found");
            group.GroupCode = request.GroupCode;
            group.GroupName = request.GroupName;
            group.Description = request.Description;
            group.IsActive = request.IsActive;
            group.UpdatedAt = DateTime.UtcNow;
            group.UpdatedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var group = await _dbContext.ItemGroups.FirstOrDefaultAsync(x => x.ItemGroupId == id && !x.IsDeleted);
            if (group == null) throw new Exception("Item group not found");
            if (await _dbContext.Items.AnyAsync(x => x.ItemGroupId == id && !x.IsDeleted)) throw new Exception("Item group is used by items and cannot be deleted");
            group.IsDeleted = true;
            group.DeletedAt = DateTime.UtcNow;
            group.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }
    }

    public interface IItemUnitService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableUnitRequest request);
        Task<UnitViewModel?> GetById(Guid id);
        Task<List<UnitViewModel>> GetActiveUnits();
        Task Create(CreateUnitRequest request, Guid userId);
        Task Update(UpdateUnitRequest request, Guid userId);
        Task Delete(Guid id, Guid userId);
    }

    public class ItemUnitService : BaseService, IItemUnitService
    {
        public ItemUnitService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<BaseDatatableResponse> Datatable(DataTableUnitRequest request)
        {
            var query = _dbContext.Units.Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.UnitCode.ToLower().Contains(search) || x.UnitName.ToLower().Contains(search));
            }
            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.UnitCode).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new UnitViewModel
                {
                    UnitId = x.UnitId,
                    UnitCode = x.UnitCode,
                    UnitName = x.UnitName,
                    Description = x.Description,
                    IsActive = x.IsActive
                }).ToListAsync();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<UnitViewModel?> GetById(Guid id)
        {
            return await _dbContext.Units
                .Where(x => x.UnitId == id && !x.IsDeleted)
                .Select(x => new UnitViewModel
                {
                    UnitId = x.UnitId,
                    UnitCode = x.UnitCode,
                    UnitName = x.UnitName,
                    Description = x.Description,
                    IsActive = x.IsActive
                }).FirstOrDefaultAsync();
        }

        public async Task<List<UnitViewModel>> GetActiveUnits()
        {
            return await _dbContext.Units
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.UnitCode)
                .Select(x => new UnitViewModel
                {
                    UnitId = x.UnitId,
                    UnitCode = x.UnitCode,
                    UnitName = x.UnitName,
                    Description = x.Description,
                    IsActive = x.IsActive
                }).ToListAsync();
        }

        public async Task Create(CreateUnitRequest request, Guid userId)
        {
            var exists = await _dbContext.Units.AnyAsync(x => x.UnitCode == request.UnitCode && !x.IsDeleted);
            if (exists) throw new Exception("Unit code already exists");
            _dbContext.Units.Add(new UnitEntity
            {
                UnitId = Guid.NewGuid(),
                UnitCode = request.UnitCode,
                UnitName = request.UnitName,
                Description = request.Description,
                IsActive = true,
                CreatedBy = userId.ToString()
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(UpdateUnitRequest request, Guid userId)
        {
            var unit = await _dbContext.Units.FirstOrDefaultAsync(x => x.UnitId == request.UnitId && !x.IsDeleted);
            if (unit == null) throw new Exception("Unit not found");
            unit.UnitCode = request.UnitCode;
            unit.UnitName = request.UnitName;
            unit.Description = request.Description;
            unit.IsActive = request.IsActive;
            unit.UpdatedAt = DateTime.UtcNow;
            unit.UpdatedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var unit = await _dbContext.Units.FirstOrDefaultAsync(x => x.UnitId == id && !x.IsDeleted);
            if (unit == null) throw new Exception("Unit not found");
            unit.IsDeleted = true;
            unit.DeletedAt = DateTime.UtcNow;
            unit.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }
    }

    public interface ITaxService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableTaxRequest request);
        Task<TaxViewModel?> GetById(Guid id);
        Task<BaseDatatableResponse> VatReport(VatReportRequest request);
        Task Create(CreateTaxRequest request, Guid userId);
        Task Update(UpdateTaxRequest request, Guid userId);
        Task Delete(Guid id, Guid userId);
    }

    public class TaxService : BaseService, ITaxService
    {
        public TaxService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<BaseDatatableResponse> Datatable(DataTableTaxRequest request)
        {
            var query = _dbContext.Taxes.Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.TaxCode.ToLower().Contains(search) || x.TaxName.ToLower().Contains(search));
            }
            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.TaxCode).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new TaxViewModel
                {
                    TaxId = x.TaxId,
                    TaxCode = x.TaxCode,
                    TaxName = x.TaxName,
                    Rate = x.Rate,
                    TaxType = x.TaxType,
                    IsActive = x.IsActive
                }).ToListAsync();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<TaxViewModel?> GetById(Guid id)
        {
            return await _dbContext.Taxes
                .Where(x => x.TaxId == id && !x.IsDeleted)
                .Select(x => new TaxViewModel
                {
                    TaxId = x.TaxId,
                    TaxCode = x.TaxCode,
                    TaxName = x.TaxName,
                    Rate = x.Rate,
                    TaxType = x.TaxType,
                    IsActive = x.IsActive
                }).FirstOrDefaultAsync();
        }

        public async Task<BaseDatatableResponse> VatReport(VatReportRequest request)
        {
            var invoices = await _dbContext.Invoices
                .Include(x => x.Customer).Include(x => x.Supplier)
                .Where(x => !x.IsDeleted && x.Status == "Posted"
                    && x.TaxAmount != 0
                    && x.InvoiceDate.Date >= request.FromDate.Date && x.InvoiceDate.Date <= request.ToDate.Date)
                .ToListAsync();
            var lines = invoices.Select(x =>
            {
                var dpp = x.TotalAmount - x.TaxAmount;
                return new VatReportLine
                {
                    InvoiceNumber = x.InvoiceNumber,
                    InvoiceType = x.InvoiceType,
                    PartnerName = x.Customer != null ? x.Customer.CustomerName : x.Supplier != null ? x.Supplier.SupplierName : "-",
                    InvoiceDate = x.InvoiceDate,
                    Dpp = dpp,
                    Vat = x.TaxAmount,
                    TotalAmount = x.TotalAmount
                };
            }).ToList();
            var summary = new VatReportSummary
            {
                OutputVat = lines.Where(x => x.InvoiceType == "Sales").Sum(x => x.Vat),
                InputVat = lines.Where(x => x.InvoiceType == "Purchase").Sum(x => x.Vat)
            };
            return new BaseDatatableResponse { Draw = 1, RecordsTotal = lines.Count, RecordsFiltered = lines.Count, Data = new { lines, summary } };
        }

        public async Task Create(CreateTaxRequest request, Guid userId)
        {
            var exists = await _dbContext.Taxes.AnyAsync(x => x.TaxCode == request.TaxCode && !x.IsDeleted);
            if (exists) throw new Exception("Tax code already exists");
            _dbContext.Taxes.Add(new TaxEntity
            {
                TaxId = Guid.NewGuid(),
                TaxCode = request.TaxCode,
                TaxName = request.TaxName,
                Rate = request.Rate,
                TaxType = request.TaxType,
                IsActive = true,
                CreatedBy = userId.ToString()
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(UpdateTaxRequest request, Guid userId)
        {
            var tax = await _dbContext.Taxes.FirstOrDefaultAsync(x => x.TaxId == request.TaxId && !x.IsDeleted);
            if (tax == null) throw new Exception("Tax not found");
            tax.TaxCode = request.TaxCode;
            tax.TaxName = request.TaxName;
            tax.Rate = request.Rate;
            tax.TaxType = request.TaxType;
            tax.IsActive = request.IsActive;
            tax.UpdatedAt = DateTime.UtcNow;
            tax.UpdatedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var tax = await _dbContext.Taxes.FirstOrDefaultAsync(x => x.TaxId == id && !x.IsDeleted);
            if (tax == null) throw new Exception("Tax not found");
            tax.IsDeleted = true;
            tax.DeletedAt = DateTime.UtcNow;
            tax.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }
    }
}

