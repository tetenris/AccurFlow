using AccuFlow.Infrastructure.Persistence;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.AgingReport;
using AccuFlow.Models.Approval;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.DocumentAttachment;
using AccuFlow.Models.Inventory;
using AccuFlow.Models.Invoice;
using AccuFlow.Models.Payment;
using AccuFlow.Models.PurchaseOrder;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface IPurchaseOrderService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTablePurchaseOrderRequest request);
        Task<PurchaseOrderDetailViewModel?> GetById(Guid id);
        Task Create(CreatePurchaseOrderRequest request, Guid userId);
        Task Edit(UpdatePurchaseOrderRequest request, Guid userId);
        Task Delete(Guid id, Guid userId);
        Task Approve(Guid id, Guid userId);
        Task ConvertToInvoice(Guid id, Guid userId);
    }

    public class PurchaseOrderService : BaseService, IPurchaseOrderService
    {
        public PurchaseOrderService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<BaseDatatableResponse> Datatable(DataTablePurchaseOrderRequest request)
        {
            var query = _dbContext.PurchaseOrders.Include(x => x.Supplier).Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            if (request.SupplierId.HasValue) query = query.Where(x => x.SupplierId == request.SupplierId);
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.OrderDate).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new PurchaseOrderViewModel
                {
                    PurchaseOrderId = x.PurchaseOrderId,
                    PurchaseOrderNumber = x.PurchaseOrderNumber,
                    OrderDate = x.OrderDate,
                    ExpectedDate = x.ExpectedDate,
                    SupplierName = x.Supplier.SupplierName,
                    Status = x.Status,
                    TotalAmount = x.TotalAmount
                }).ToListAsync();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<PurchaseOrderDetailViewModel?> GetById(Guid id)
        {
            return await _dbContext.PurchaseOrders
                .Include(x => x.Supplier)
                .Include(x => x.PurchaseInvoice)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                    .ThenInclude(x => x.Item)
                .Where(x => x.PurchaseOrderId == id && !x.IsDeleted)
                .Select(x => new PurchaseOrderDetailViewModel
                {
                    PurchaseOrderId = x.PurchaseOrderId,
                    PurchaseOrderNumber = x.PurchaseOrderNumber,
                    OrderDate = x.OrderDate,
                    ExpectedDate = x.ExpectedDate,
                    SupplierId = x.SupplierId,
                    SupplierName = x.Supplier.SupplierName,
                    Status = x.Status,
                    TotalAmount = x.TotalAmount,
                    PurchaseInvoiceId = x.PurchaseInvoiceId,
                    PurchaseInvoiceNumber = x.PurchaseInvoice != null ? x.PurchaseInvoice.InvoiceNumber : null,
                    Notes = x.Notes,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanApprove = x.Status == "Draft",
                    CanConvert = x.Status == "Approved" && !x.PurchaseInvoiceId.HasValue,
                    Lines = x.Lines.Where(l => !l.IsDeleted).Select(l => new PurchaseOrderLineViewModel
                    {
                        PurchaseOrderLineId = l.PurchaseOrderLineId,
                        ItemId = l.ItemId,
                        ItemName = l.Item != null ? l.Item.ItemName : null,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        TaxAmount = l.TaxAmount,
                        LineTotal = l.LineTotal
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task Create(CreatePurchaseOrderRequest request, Guid userId)
        {
            var po = new PurchaseOrderEntity
            {
                PurchaseOrderId = Guid.NewGuid(),
                PurchaseOrderNumber = await GenerateNumber(),
                OrderDate = request.OrderDate,
                ExpectedDate = request.ExpectedDate,
                SupplierId = request.SupplierId,
                Status = "Draft",
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            };
            foreach (var line in request.Lines)
            {
                var lineTotal = (line.Quantity * line.UnitPrice) + line.TaxAmount;
                po.Lines.Add(new PurchaseOrderLineEntity
                {
                    PurchaseOrderLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                po.SubTotal += line.Quantity * line.UnitPrice;
                po.TaxAmount += line.TaxAmount;
                po.TotalAmount += lineTotal;
            }
            _dbContext.PurchaseOrders.Add(po);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Edit(UpdatePurchaseOrderRequest request, Guid userId)
        {
            var po = await _dbContext.PurchaseOrders
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.PurchaseOrderId == request.PurchaseOrderId && !x.IsDeleted);
            if (po == null) throw new Exception("Purchase order not found");
            if (po.Status != "Draft") throw new Exception("Only draft purchase order can be edited");
            if (!request.Lines.Any()) throw new Exception("Purchase order lines are required");

            po.OrderDate = request.OrderDate;
            po.ExpectedDate = request.ExpectedDate;
            po.SupplierId = request.SupplierId;
            po.Notes = request.Notes;
            po.SubTotal = 0;
            po.TaxAmount = 0;
            po.TotalAmount = 0;
            po.UpdatedBy = userId.ToString();
            po.UpdatedAt = DateTime.UtcNow;

            foreach (var oldLine in po.Lines)
            {
                oldLine.IsDeleted = true;
                oldLine.DeletedBy = userId.ToString();
                oldLine.DeletedAt = DateTime.UtcNow;
            }

            foreach (var line in request.Lines)
            {
                var lineTotal = (line.Quantity * line.UnitPrice) + line.TaxAmount;
                po.Lines.Add(new PurchaseOrderLineEntity
                {
                    PurchaseOrderLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                po.SubTotal += line.Quantity * line.UnitPrice;
                po.TaxAmount += line.TaxAmount;
                po.TotalAmount += lineTotal;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var po = await _dbContext.PurchaseOrders.Include(x => x.Lines).FirstOrDefaultAsync(x => x.PurchaseOrderId == id && !x.IsDeleted);
            if (po == null) throw new Exception("Purchase order not found");
            if (po.Status != "Draft") throw new Exception("Only draft purchase order can be deleted");
            po.IsDeleted = true;
            po.DeletedBy = userId.ToString();
            po.DeletedAt = DateTime.UtcNow;
            foreach (var line in po.Lines)
            {
                line.IsDeleted = true;
                line.DeletedBy = userId.ToString();
                line.DeletedAt = DateTime.UtcNow;
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task Approve(Guid id, Guid userId)
        {
            var po = await _dbContext.PurchaseOrders.FirstOrDefaultAsync(x => x.PurchaseOrderId == id && !x.IsDeleted);
            if (po == null) throw new Exception("Purchase order not found");
            if (po.Status != "Draft") throw new Exception("Only draft purchase order can be approved");
            po.Status = "Approved";
            po.UpdatedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task ConvertToInvoice(Guid id, Guid userId)
        {
            var po = await _dbContext.PurchaseOrders.Include(x => x.Lines).FirstOrDefaultAsync(x => x.PurchaseOrderId == id && !x.IsDeleted);
            if (po == null) throw new Exception("Purchase order not found");
            if (po.PurchaseInvoiceId.HasValue) throw new Exception("Purchase order already converted");
            var invoice = new InvoiceEntity
            {
                InvoiceId = Guid.NewGuid(),
                InvoiceNumber = $"PI-{DateTime.UtcNow:yyyyMMddHHmmss}",
                InvoiceType = "Purchase",
                InvoiceDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(30),
                SupplierId = po.SupplierId,
                Status = "Draft",
                SubTotal = po.SubTotal,
                TaxAmount = po.TaxAmount,
                TotalAmount = po.TotalAmount,
                CreatedBy = userId.ToString()
            };
            foreach (var line in po.Lines)
            {
                invoice.Lines.Add(new InvoiceLineEntity
                {
                    InvoiceLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    LineTotal = line.LineTotal,
                    CreatedBy = userId.ToString()
                });
            }
            po.PurchaseInvoiceId = invoice.InvoiceId;
            po.Status = "Converted";
            _dbContext.Invoices.Add(invoice);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<string> GenerateNumber()
        {
            var last = await _dbContext.PurchaseOrders.Where(x => x.PurchaseOrderNumber.StartsWith("PO-")).OrderByDescending(x => x.PurchaseOrderNumber).FirstOrDefaultAsync();
            var next = last == null ? 1 : int.Parse(last.PurchaseOrderNumber[3..]) + 1;
            return $"PO-{next:D5}";
        }
    }
}


