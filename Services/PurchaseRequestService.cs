using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.PurchaseRequest;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface IPurchaseRequestService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTablePurchaseRequestRequest request);
        Task<PurchaseRequestDetailViewModel?> GetById(Guid id);
        Task Create(CreatePurchaseRequestRequest request, Guid userId);
        Task Update(UpdatePurchaseRequestRequest request, Guid userId);
        Task Delete(Guid id, Guid userId);
        Task Approve(Guid id, Guid userId);
        Task<Guid> ConvertToPurchaseOrder(Guid id, Guid supplierId, DateTime? expectedDate, Guid userId);
    }

    public class PurchaseRequestService : BaseService, IPurchaseRequestService
    {
        public PurchaseRequestService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<BaseDatatableResponse> Datatable(DataTablePurchaseRequestRequest request)
        {
            var query = _dbContext.PurchaseRequests
                .Include(x => x.PurchaseOrder)
                .Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            if (request.DateFrom.HasValue) query = query.Where(x => x.RequestDate >= request.DateFrom.Value.Date);
            if (request.DateTo.HasValue) query = query.Where(x => x.RequestDate <= request.DateTo.Value.Date);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.PurchaseRequestNumber.ToLower().Contains(search) || x.RequestedBy.ToLower().Contains(search) || (x.Department != null && x.Department.ToLower().Contains(search)));
            }
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.RequestDate).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new PurchaseRequestViewModel
                {
                    PurchaseRequestId = x.PurchaseRequestId,
                    PurchaseRequestNumber = x.PurchaseRequestNumber,
                    RequestDate = x.RequestDate,
                    RequiredDate = x.RequiredDate,
                    RequestedBy = x.RequestedBy,
                    Department = x.Department,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount,
                    PurchaseOrderNumber = x.PurchaseOrder != null ? x.PurchaseOrder.PurchaseOrderNumber : null
                }).ToListAsync();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<PurchaseRequestDetailViewModel?> GetById(Guid id)
        {
            return await _dbContext.PurchaseRequests
                .Include(x => x.PurchaseOrder)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.PurchaseRequestId == id && !x.IsDeleted)
                .Select(x => new PurchaseRequestDetailViewModel
                {
                    PurchaseRequestId = x.PurchaseRequestId,
                    PurchaseRequestNumber = x.PurchaseRequestNumber,
                    RequestDate = x.RequestDate,
                    RequiredDate = x.RequiredDate,
                    RequestedBy = x.RequestedBy,
                    Department = x.Department,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount,
                    Notes = x.Notes,
                    PurchaseOrderId = x.PurchaseOrderId,
                    PurchaseOrderNumber = x.PurchaseOrder != null ? x.PurchaseOrder.PurchaseOrderNumber : null,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanApprove = x.Status == "Draft",
                    CanConvert = x.Status == "Approved" && !x.PurchaseOrderId.HasValue,
                    Lines = x.Lines.Where(l => !l.IsDeleted).Select(l => new PurchaseRequestLineViewModel
                    {
                        PurchaseRequestLineId = l.PurchaseRequestLineId,
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

        public async Task Create(CreatePurchaseRequestRequest request, Guid userId)
        {
            if (request.Lines == null || request.Lines.Count == 0) throw new Exception("At least one item line is required");

            var pr = new PurchaseRequestEntity
            {
                PurchaseRequestId = Guid.NewGuid(),
                PurchaseRequestNumber = await GenerateNumberAsync(),
                RequestDate = request.RequestDate,
                RequiredDate = request.RequiredDate,
                RequestedBy = string.IsNullOrWhiteSpace(request.RequestedBy) ? await GetCurrentUserNameAsync(userId) : request.RequestedBy,
                Department = request.Department,
                Status = "Draft",
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            };
            FillTotals(pr, request.Lines, userId);
            _dbContext.PurchaseRequests.Add(pr);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(UpdatePurchaseRequestRequest request, Guid userId)
        {
            var pr = await _dbContext.PurchaseRequests.Include(x => x.Lines).FirstOrDefaultAsync(x => x.PurchaseRequestId == request.PurchaseRequestId && !x.IsDeleted);
            if (pr == null) throw new Exception("Purchase request not found");
            if (pr.Status != "Draft") throw new Exception("Only draft purchase request can be edited");
            if (request.Lines == null || request.Lines.Count == 0) throw new Exception("At least one item line is required");

            pr.RequestDate = request.RequestDate;
            pr.RequiredDate = request.RequiredDate;
            pr.RequestedBy = string.IsNullOrWhiteSpace(request.RequestedBy) ? pr.RequestedBy : request.RequestedBy;
            pr.Department = request.Department;
            pr.Notes = request.Notes;
            pr.UpdatedAt = DateTime.UtcNow;
            pr.UpdatedBy = userId.ToString();
            foreach (var existing in pr.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true; existing.DeletedAt = DateTime.UtcNow; existing.DeletedBy = userId.ToString();
            }
            ResetTotals(pr);
            FillTotals(pr, request.Lines, userId);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var pr = await _dbContext.PurchaseRequests.FirstOrDefaultAsync(x => x.PurchaseRequestId == id && !x.IsDeleted);
            if (pr == null) throw new Exception("Purchase request not found");
            if (pr.Status != "Draft") throw new Exception("Only draft purchase request can be deleted");
            pr.IsDeleted = true; pr.DeletedAt = DateTime.UtcNow; pr.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task Approve(Guid id, Guid userId)
        {
            var pr = await _dbContext.PurchaseRequests.FirstOrDefaultAsync(x => x.PurchaseRequestId == id && !x.IsDeleted);
            if (pr == null) throw new Exception("Purchase request not found");
            if (pr.Status != "Draft") throw new Exception("Only draft purchase request can be approved");
            pr.Status = "Approved";
            pr.UpdatedAt = DateTime.UtcNow;
            pr.UpdatedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Guid> ConvertToPurchaseOrder(Guid id, Guid supplierId, DateTime? expectedDate, Guid userId)
        {
            var pr = await _dbContext.PurchaseRequests
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .FirstOrDefaultAsync(x => x.PurchaseRequestId == id && !x.IsDeleted);
            if (pr == null) throw new Exception("Purchase request not found");
            if (pr.Status != "Approved") throw new Exception("Only approved purchase request can be converted");
            if (pr.PurchaseOrderId.HasValue) throw new Exception("Purchase request already converted to purchase order");
            if (supplierId == Guid.Empty) throw new Exception("Supplier is required");

            var po = new PurchaseOrderEntity
            {
                PurchaseOrderId = Guid.NewGuid(),
                PurchaseOrderNumber = await GeneratePoNumberAsync(),
                OrderDate = DateTime.Today,
                ExpectedDate = expectedDate,
                SupplierId = supplierId,
                Status = "Draft",
                Notes = $"Created from purchase request {pr.PurchaseRequestNumber}" + (string.IsNullOrWhiteSpace(pr.Notes) ? "" : $" - {pr.Notes}"),
                CreatedBy = userId.ToString()
            };
            foreach (var line in pr.Lines.Where(l => !l.IsDeleted))
            {
                if (line.Quantity <= 0) continue;
                po.Lines.Add(new PurchaseOrderLineEntity
                {
                    PurchaseOrderLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    LineTotal = line.LineTotal,
                    CreatedBy = userId.ToString()
                });
                po.SubTotal += line.Quantity * line.UnitPrice;
                po.TaxAmount += line.TaxAmount;
                po.TotalAmount += line.LineTotal;
            }
            if (po.Lines.Count == 0) throw new Exception("Purchase request has no items to convert");

            pr.PurchaseOrderId = po.PurchaseOrderId;
            pr.Status = "Converted";
            pr.UpdatedAt = DateTime.UtcNow;
            pr.UpdatedBy = userId.ToString();
            _dbContext.PurchaseOrders.Add(po);
            await _dbContext.SaveChangesAsync();
            return po.PurchaseOrderId;
        }

        private void FillTotals(PurchaseRequestEntity pr, IEnumerable<CreatePurchaseRequestLineRequest> lines, Guid userId)
        {
            foreach (var line in lines)
            {
                if (line.Quantity <= 0) continue;
                var lineTotal = (line.Quantity * line.UnitPrice) + line.TaxAmount;
                pr.Lines.Add(new PurchaseRequestLineEntity
                {
                    PurchaseRequestLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                pr.SubTotal += line.Quantity * line.UnitPrice;
                pr.TaxAmount += line.TaxAmount;
                pr.TotalAmount += lineTotal;
            }
        }

        private void ResetTotals(PurchaseRequestEntity pr)
        {
            pr.SubTotal = 0; pr.TaxAmount = 0; pr.TotalAmount = 0;
        }

        private async Task<string> GetCurrentUserNameAsync(Guid userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);
            return user != null && !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user?.UserName ?? userId.ToString();
        }

        private async Task<string> GenerateNumberAsync()
        {
            var last = await _dbContext.PurchaseRequests.Where(x => x.PurchaseRequestNumber.StartsWith("PR-"))
                .OrderByDescending(x => x.PurchaseRequestNumber).Select(x => x.PurchaseRequestNumber).FirstOrDefaultAsync();
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[3..]) + 1;
            return $"PR-{next:D5}";
        }

        private async Task<string> GeneratePoNumberAsync()
        {
            var last = await _dbContext.PurchaseOrders.Where(x => x.PurchaseOrderNumber.StartsWith("PO-"))
                .OrderByDescending(x => x.PurchaseOrderNumber).Select(x => x.PurchaseOrderNumber).FirstOrDefaultAsync();
            var next = last == null ? 1 : int.Parse(last[3..]) + 1;
            return $"PO-{next:D5}";
        }
    }
}