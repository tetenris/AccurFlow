using AccuFlow.Entities.Context;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Delivery;
using AccuFlow.Models.Quotation;
using AccuFlow.Models.SalesOrder;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface IQuotationService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableQuotationRequest request);
        Task<QuotationDetailViewModel?> GetById(Guid id);
        Task Create(CreateQuotationRequest request, Guid userId);
        Task Update(UpdateQuotationRequest request, Guid userId);
        Task Delete(Guid id, Guid userId);
        Task Approve(Guid id, Guid userId);
        Task<List<QuoteOptionViewModel>> GetApprovedQuotes();
    }

    public class QuotationService : BaseService, IQuotationService
    {
        public QuotationService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<BaseDatatableResponse> Datatable(DataTableQuotationRequest request)
        {
            var query = _dbContext.SalesQuotations.Include(x => x.Customer).Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.QuotationNumber.ToLower().Contains(search) || x.Customer.CustomerName.ToLower().Contains(search));
            }
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.QuotationDate).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new QuotationViewModel
                {
                    SalesQuotationId = x.SalesQuotationId,
                    QuotationNumber = x.QuotationNumber,
                    QuotationDate = x.QuotationDate,
                    ValidUntil = x.ValidUntil,
                    CustomerName = x.Customer.CustomerName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount
                }).ToListAsync();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<QuotationDetailViewModel?> GetById(Guid id)
        {
            return await _dbContext.SalesQuotations
                .Include(x => x.Customer)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.SalesQuotationId == id && !x.IsDeleted)
                .Select(x => new QuotationDetailViewModel
                {
                    SalesQuotationId = x.SalesQuotationId,
                    QuotationNumber = x.QuotationNumber,
                    QuotationDate = x.QuotationDate,
                    ValidUntil = x.ValidUntil,
                    CustomerName = x.Customer.CustomerName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount,
                    Notes = x.Notes,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanApprove = x.Status == "Draft",
                    Lines = x.Lines.Select(l => new QuotationLineViewModel
                    {
                        SalesQuotationLineId = l.SalesQuotationLineId,
                        ItemId = l.ItemId,
                        ItemCode = l.Item != null ? l.Item.ItemCode : string.Empty,
                        ItemName = l.Item != null ? l.Item.ItemName : string.Empty,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        DiscountAmount = l.DiscountAmount,
                        TaxAmount = l.TaxAmount,
                        LineTotal = l.LineTotal
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task Create(CreateQuotationRequest request, Guid userId)
        {
            if (request.CustomerId == Guid.Empty) throw new Exception("Customer is required");
            if (request.Lines == null || request.Lines.Count == 0) throw new Exception("At least one item line is required");

            var quote = new SalesQuotationEntity
            {
                SalesQuotationId = Guid.NewGuid(),
                QuotationNumber = await GenerateNumberAsync(),
                QuotationDate = request.QuotationDate,
                ValidUntil = request.ValidUntil,
                CustomerId = request.CustomerId,
                Status = "Draft",
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            };
            FillTotals(quote, request.Lines, userId);
            _dbContext.SalesQuotations.Add(quote);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(UpdateQuotationRequest request, Guid userId)
        {
            var quote = await _dbContext.SalesQuotations.Include(x => x.Lines).FirstOrDefaultAsync(x => x.SalesQuotationId == request.SalesQuotationId && !x.IsDeleted);
            if (quote == null) throw new Exception("Quotation not found");
            if (quote.Status != "Draft") throw new Exception("Only draft quotation can be edited");

            quote.QuotationDate = request.QuotationDate;
            quote.ValidUntil = request.ValidUntil;
            quote.CustomerId = request.CustomerId;
            quote.Notes = request.Notes;
            foreach (var existing in quote.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true; existing.DeletedAt = DateTime.UtcNow; existing.DeletedBy = userId.ToString();
            }
            ResetTotals(quote);
            FillTotals(quote, request.Lines, userId);
            quote.UpdatedAt = DateTime.UtcNow;
            quote.UpdatedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var quote = await _dbContext.SalesQuotations.FirstOrDefaultAsync(x => x.SalesQuotationId == id && !x.IsDeleted);
            if (quote == null) throw new Exception("Quotation not found");
            if (quote.Status != "Draft") throw new Exception("Only draft quotation can be deleted");
            quote.IsDeleted = true; quote.DeletedAt = DateTime.UtcNow; quote.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task Approve(Guid id, Guid userId)
        {
            var quote = await _dbContext.SalesQuotations.FirstOrDefaultAsync(x => x.SalesQuotationId == id && !x.IsDeleted);
            if (quote == null) throw new Exception("Quotation not found");
            if (quote.Status != "Draft") throw new Exception("Only draft quotation can be approved");
            quote.Status = "Approved";
            quote.UpdatedAt = DateTime.UtcNow;
            quote.UpdatedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<QuoteOptionViewModel>> GetApprovedQuotes()
        {
            return await _dbContext.SalesQuotations
                .Include(x => x.Customer)
                .Where(x => x.Status == "Approved" && !x.IsDeleted)
                .OrderByDescending(x => x.QuotationDate)
                .Select(x => new QuoteOptionViewModel
                {
                    SalesQuotationId = x.SalesQuotationId,
                    QuotationNumber = x.QuotationNumber,
                    CustomerName = x.Customer.CustomerName,
                    QuotationDate = x.QuotationDate
                }).ToListAsync();
        }

        private void FillTotals(SalesQuotationEntity quote, IEnumerable<QuotationLineRequest> lines, Guid userId)
        {
            foreach (var line in lines)
            {
                if (line.Quantity <= 0) continue;
                var lineTotal = (line.Quantity * line.UnitPrice) - line.DiscountAmount + line.TaxAmount;
                quote.Lines.Add(new SalesQuotationLineEntity
                {
                    SalesQuotationLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    DiscountAmount = line.DiscountAmount,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                quote.SubTotal += line.Quantity * line.UnitPrice;
                quote.DiscountAmount += line.DiscountAmount;
                quote.TaxAmount += line.TaxAmount;
                quote.TotalAmount += lineTotal;
            }
        }

        private void ResetTotals(SalesQuotationEntity quote)
        {
            quote.SubTotal = 0; quote.DiscountAmount = 0; quote.TaxAmount = 0; quote.TotalAmount = 0;
        }

        private async Task<string> GenerateNumberAsync()
        {
            var last = await _dbContext.SalesQuotations.Where(x => x.QuotationNumber.StartsWith("QT-"))
                .OrderByDescending(x => x.QuotationNumber).Select(x => x.QuotationNumber).FirstOrDefaultAsync();
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[3..]) + 1;
            return $"QT-{next:D5}";
        }
    }

    public interface ISalesOrderService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableOrderRequest request);
        Task<OrderDetailViewModel?> GetById(Guid id);
        Task Create(CreateOrderRequest request, Guid userId);
        Task Update(UpdateOrderRequest request, Guid userId);
        Task Delete(Guid id, Guid userId);
        Task Approve(Guid id, Guid userId);
        Task<QuotationDetailViewModel?> GetQuoteById(Guid id);
        Task<List<QuoteOptionViewModel>> GetApprovedQuotes();
    }

    public class SalesOrderService : BaseService, ISalesOrderService
    {
        public SalesOrderService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<BaseDatatableResponse> Datatable(DataTableOrderRequest request)
        {
            var query = _dbContext.SalesOrders
                .Include(x => x.Customer).Include(x => x.Quotation).Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.OrderNumber.ToLower().Contains(search) || x.Customer.CustomerName.ToLower().Contains(search));
            }
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.OrderDate).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new OrderViewModel
                {
                    SalesOrderId = x.SalesOrderId,
                    OrderNumber = x.OrderNumber,
                    OrderDate = x.OrderDate,
                    ExpectedDate = x.ExpectedDate,
                    CustomerName = x.Customer.CustomerName,
                    QuotationNumber = x.Quotation != null ? x.Quotation.QuotationNumber : string.Empty,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount
                }).ToListAsync();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<OrderDetailViewModel?> GetById(Guid id)
        {
            return await _dbContext.SalesOrders
                .Include(x => x.Customer).Include(x => x.Quotation)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.SalesOrderId == id && !x.IsDeleted)
                .Select(x => new OrderDetailViewModel
                {
                    SalesOrderId = x.SalesOrderId,
                    OrderNumber = x.OrderNumber,
                    OrderDate = x.OrderDate,
                    ExpectedDate = x.ExpectedDate,
                    CustomerId = x.CustomerId,
                    CustomerName = x.Customer.CustomerName,
                    QuotationId = x.QuotationId,
                    QuotationNumber = x.Quotation != null ? x.Quotation.QuotationNumber : string.Empty,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount,
                    Notes = x.Notes,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanApprove = x.Status == "Draft",
                    Lines = x.Lines.Select(l => new OrderLineViewModel
                    {
                        SalesOrderLineId = l.SalesOrderLineId,
                        ItemId = l.ItemId,
                        ItemCode = l.Item != null ? l.Item.ItemCode : string.Empty,
                        ItemName = l.Item != null ? l.Item.ItemName : string.Empty,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        DiscountAmount = l.DiscountAmount,
                        TaxAmount = l.TaxAmount,
                        LineTotal = l.LineTotal
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task Create(CreateOrderRequest request, Guid userId)
        {
            if (request.CustomerId == Guid.Empty) throw new Exception("Customer is required");
            if (request.Lines == null || request.Lines.Count == 0) throw new Exception("At least one item line is required");

            var order = new SalesOrderEntity
            {
                SalesOrderId = Guid.NewGuid(),
                OrderNumber = await GenerateNumberAsync(),
                OrderDate = request.OrderDate,
                ExpectedDate = request.ExpectedDate,
                CustomerId = request.CustomerId,
                QuotationId = request.QuotationId,
                Status = "Draft",
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            };
            FillTotals(order, request.Lines, userId);
            _dbContext.SalesOrders.Add(order);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(UpdateOrderRequest request, Guid userId)
        {
            var order = await _dbContext.SalesOrders.Include(x => x.Lines).FirstOrDefaultAsync(x => x.SalesOrderId == request.SalesOrderId && !x.IsDeleted);
            if (order == null) throw new Exception("Sales order not found");
            if (order.Status != "Draft") throw new Exception("Only draft sales order can be edited");

            order.OrderDate = request.OrderDate;
            order.ExpectedDate = request.ExpectedDate;
            order.CustomerId = request.CustomerId;
            order.QuotationId = request.QuotationId;
            order.Notes = request.Notes;
            foreach (var existing in order.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true; existing.DeletedAt = DateTime.UtcNow; existing.DeletedBy = userId.ToString();
            }
            ResetTotals(order);
            FillTotals(order, request.Lines, userId);
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var order = await _dbContext.SalesOrders.FirstOrDefaultAsync(x => x.SalesOrderId == id && !x.IsDeleted);
            if (order == null) throw new Exception("Sales order not found");
            if (order.Status != "Draft") throw new Exception("Only draft sales order can be deleted");
            order.IsDeleted = true; order.DeletedAt = DateTime.UtcNow; order.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task Approve(Guid id, Guid userId)
        {
            var order = await _dbContext.SalesOrders.FirstOrDefaultAsync(x => x.SalesOrderId == id && !x.IsDeleted);
            if (order == null) throw new Exception("Sales order not found");
            if (order.Status != "Draft") throw new Exception("Only draft sales order can be approved");
            order.Status = "Approved";
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<QuoteOptionViewModel>> GetApprovedQuotes()
        {
            return await _dbContext.SalesQuotations
                .Include(x => x.Customer)
                .Where(x => x.Status == "Approved" && !x.IsDeleted)
                .OrderByDescending(x => x.QuotationDate)
                .Select(x => new QuoteOptionViewModel
                {
                    SalesQuotationId = x.SalesQuotationId,
                    QuotationNumber = x.QuotationNumber,
                    CustomerName = x.Customer.CustomerName,
                    QuotationDate = x.QuotationDate
                }).ToListAsync();
        }

        public async Task<QuotationDetailViewModel?> GetQuoteById(Guid id)
        {
            return await _dbContext.SalesQuotations
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.SalesQuotationId == id && !x.IsDeleted && x.Status == "Approved")
                .Select(x => new QuotationDetailViewModel
                {
                    SalesQuotationId = x.SalesQuotationId,
                    CustomerId = x.CustomerId,
                    Lines = x.Lines.Select(l => new QuotationLineViewModel
                    {
                        ItemId = l.ItemId,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        DiscountAmount = l.DiscountAmount,
                        TaxAmount = l.TaxAmount,
                        LineTotal = l.LineTotal
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        private void FillTotals(SalesOrderEntity order, IEnumerable<OrderLineRequest> lines, Guid userId)
        {
            foreach (var line in lines)
            {
                if (line.Quantity <= 0) continue;
                var lineTotal = (line.Quantity * line.UnitPrice) - line.DiscountAmount + line.TaxAmount;
                order.Lines.Add(new SalesOrderLineEntity
                {
                    SalesOrderLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    DiscountAmount = line.DiscountAmount,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                order.SubTotal += line.Quantity * line.UnitPrice;
                order.DiscountAmount += line.DiscountAmount;
                order.TaxAmount += line.TaxAmount;
                order.TotalAmount += lineTotal;
            }
        }

        private void ResetTotals(SalesOrderEntity order)
        {
            order.SubTotal = 0; order.DiscountAmount = 0; order.TaxAmount = 0; order.TotalAmount = 0;
        }

        private async Task<string> GenerateNumberAsync()
        {
            var last = await _dbContext.SalesOrders.Where(x => x.OrderNumber.StartsWith("SO-"))
                .OrderByDescending(x => x.OrderNumber).Select(x => x.OrderNumber).FirstOrDefaultAsync();
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[3..]) + 1;
            return $"SO-{next:D5}";
        }
    }

    public interface IDeliveryOrderService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableDeliveryRequest request);
        Task<DeliveryDetailViewModel?> GetById(Guid id);
        Task<List<OrderOptionViewModel>> GetOrders();
        Task<OrderDeliveryViewModel> GetOrderLines(Guid salesOrderId);
        Task Create(CreateDeliveryRequest request, Guid userId);
        Task Update(UpdateDeliveryRequest request, Guid userId);
        Task Post(Guid id, Guid userId);
        Task Delete(Guid id, Guid userId);
        Task ConvertToInvoice(Guid id, Guid userId);
    }

    public class DeliveryOrderService : BaseService, IDeliveryOrderService
    {
        public DeliveryOrderService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<BaseDatatableResponse> Datatable(DataTableDeliveryRequest request)
        {
            var query = _dbContext.DeliveryOrders
                .Include(x => x.SalesOrder).Include(x => x.Customer).Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.DeliveryNumber.ToLower().Contains(search) || x.SalesOrder.OrderNumber.ToLower().Contains(search) || x.Customer.CustomerName.ToLower().Contains(search));
            }
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.DeliveryDate).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new DeliveryViewModel
                {
                    DeliveryOrderId = x.DeliveryOrderId,
                    DeliveryNumber = x.DeliveryNumber,
                    DeliveryDate = x.DeliveryDate,
                    OrderNumber = x.SalesOrder.OrderNumber,
                    CustomerName = x.Customer.CustomerName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount
                }).ToListAsync();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<DeliveryDetailViewModel?> GetById(Guid id)
        {
            return await _dbContext.DeliveryOrders
                .Include(x => x.SalesOrder).Include(x => x.Customer).Include(x => x.Invoice)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.DeliveryOrderId == id && !x.IsDeleted)
                .Select(x => new DeliveryDetailViewModel
                {
                    DeliveryOrderId = x.DeliveryOrderId,
                    DeliveryNumber = x.DeliveryNumber,
                    DeliveryDate = x.DeliveryDate,
                    SalesOrderId = x.SalesOrderId,
                    OrderNumber = x.SalesOrder.OrderNumber,
                    CustomerName = x.Customer.CustomerName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount,
                    Notes = x.Notes,
                    InvoiceNumber = x.Invoice != null ? x.Invoice.InvoiceNumber : null,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanPost = x.Status == "Draft",
                    CanInvoice = x.Status == "Posted" && !x.InvoiceId.HasValue,
                    Lines = x.Lines.Select(l => new DeliveryLineViewModel
                    {
                        DeliveryOrderLineId = l.DeliveryOrderLineId,
                        SalesOrderLineId = l.SalesOrderLineId,
                        ItemId = l.ItemId,
                        ItemCode = l.Item != null ? l.Item.ItemCode : string.Empty,
                        ItemName = l.Item != null ? l.Item.ItemName : string.Empty,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        LineTotal = l.LineTotal
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task<List<OrderOptionViewModel>> GetOrders()
        {
            var deliveredByLine = await GetDeliveredBySoLineAsync();
            var orders = await _dbContext.SalesOrders
                .Include(x => x.Customer)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .Where(x => x.Status == "Approved" && !x.IsDeleted)
                .OrderByDescending(x => x.OrderDate)
                .Select(x => new
                {
                    x.SalesOrderId,
                    x.OrderNumber,
                    x.OrderDate,
                    x.Customer.CustomerName,
                    Lines = x.Lines.Select(l => new { l.SalesOrderLineId, l.Quantity }).ToList()
                }).ToListAsync();

            var result = new List<OrderOptionViewModel>();
            foreach (var o in orders)
            {
                var remaining = o.Lines.Sum(l =>
                {
                    var delivered = deliveredByLine.GetValueOrDefault(l.SalesOrderLineId, 0);
                    return Math.Max(0, l.Quantity - delivered);
                });
                if (remaining <= 0) continue;
                result.Add(new OrderOptionViewModel { SalesOrderId = o.SalesOrderId, OrderNumber = o.OrderNumber, CustomerName = o.CustomerName, OrderDate = o.OrderDate });
            }
            return result;
        }

        public async Task<OrderDeliveryViewModel> GetOrderLines(Guid salesOrderId)
        {
            var order = await _dbContext.SalesOrders
                .Include(x => x.Customer)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .FirstOrDefaultAsync(x => x.SalesOrderId == salesOrderId && !x.IsDeleted);
            if (order == null) throw new Exception("Sales order not found");

            var deliveredByLine = await GetDeliveredBySoLineAsync();
            var lines = order.Lines.Where(l => !l.IsDeleted).Select(l =>
            {
                var delivered = deliveredByLine.GetValueOrDefault(l.SalesOrderLineId, 0);
                return new OrderDeliveryLineViewModel
                {
                    SalesOrderLineId = l.SalesOrderLineId,
                    ItemId = l.ItemId,
                    ItemCode = l.Item != null ? l.Item.ItemCode : string.Empty,
                    ItemName = l.Item != null ? l.Item.ItemName : string.Empty,
                    Description = l.Description,
                    Quantity = l.Quantity,
                    DeliveredQuantity = delivered,
                    RemainingQuantity = l.Quantity - delivered,
                    UnitPrice = l.UnitPrice
                };
            }).Where(x => x.RemainingQuantity > 0).ToList();

            return new OrderDeliveryViewModel
            {
                SalesOrderId = order.SalesOrderId,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer.CustomerName,
                OrderDate = order.OrderDate,
                Lines = lines
            };
        }

        public async Task Create(CreateDeliveryRequest request, Guid userId)
        {
            if (request.SalesOrderId == Guid.Empty) throw new Exception("Sales order is required");
            if (request.Lines == null || request.Lines.Count == 0) throw new Exception("At least one item line is required");

            var order = await _dbContext.SalesOrders.Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.SalesOrderId == request.SalesOrderId && !x.IsDeleted);
            if (order == null) throw new Exception("Sales order not found");
            if (order.Status != "Approved") throw new Exception("Only approved sales order can be delivered");

            var doLineIds = order.Lines.Select(l => l.SalesOrderLineId).ToHashSet();
            if (request.Lines.Any(l => !doLineIds.Contains(l.SalesOrderLineId)))
                throw new Exception("Item line does not belong to the selected sales order");

            var delivery = new DeliveryOrderEntity
            {
                DeliveryOrderId = Guid.NewGuid(),
                DeliveryNumber = await GenerateNumberAsync(),
                DeliveryDate = request.DeliveryDate,
                SalesOrderId = request.SalesOrderId,
                CustomerId = order.CustomerId,
                Status = "Draft",
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            };

            foreach (var line in request.Lines)
            {
                if (line.Quantity <= 0) continue;
                var lineTotal = line.Quantity * line.UnitPrice;
                delivery.Lines.Add(new DeliveryOrderLineEntity
                {
                    DeliveryOrderLineId = Guid.NewGuid(),
                    SalesOrderLineId = line.SalesOrderLineId,
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                delivery.TotalAmount += lineTotal;
            }

            if (delivery.Lines.Count == 0) throw new Exception("At least one item line is required");
            _dbContext.DeliveryOrders.Add(delivery);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(UpdateDeliveryRequest request, Guid userId)
        {
            var delivery = await _dbContext.DeliveryOrders.Include(x => x.Lines).FirstOrDefaultAsync(x => x.DeliveryOrderId == request.DeliveryOrderId && !x.IsDeleted);
            if (delivery == null) throw new Exception("Delivery order not found");
            if (delivery.Status != "Draft") throw new Exception("Only draft delivery order can be edited");

            var order = await _dbContext.SalesOrders.Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.SalesOrderId == request.SalesOrderId && !x.IsDeleted);
            if (order == null) throw new Exception("Sales order not found");

            delivery.SalesOrderId = request.SalesOrderId;
            delivery.CustomerId = order.CustomerId;
            delivery.DeliveryDate = request.DeliveryDate;
            delivery.Notes = request.Notes;
            delivery.TotalAmount = 0;
            foreach (var existing in delivery.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true; existing.DeletedAt = DateTime.UtcNow; existing.DeletedBy = userId.ToString();
            }
            foreach (var line in request.Lines)
            {
                if (line.Quantity <= 0) continue;
                var lineTotal = line.Quantity * line.UnitPrice;
                delivery.Lines.Add(new DeliveryOrderLineEntity
                {
                    DeliveryOrderLineId = Guid.NewGuid(),
                    SalesOrderLineId = line.SalesOrderLineId,
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                delivery.TotalAmount += lineTotal;
            }
            delivery.UpdatedAt = DateTime.UtcNow;
            delivery.UpdatedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task Post(Guid id, Guid userId)
        {
            var delivery = await _dbContext.DeliveryOrders
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.DeliveryOrderId == id && !x.IsDeleted);
            if (delivery == null) throw new Exception("Delivery order not found");
            if (delivery.Status == "Posted") throw new Exception("Delivery order is already posted");
            if (delivery.Status != "Draft") throw new Exception("Only draft delivery order can be posted");

            var deliveredByLine = await GetDeliveredBySoLineAsync();
            foreach (var line in delivery.Lines)
            {
                var soLine = await _dbContext.SalesOrderLines.FirstOrDefaultAsync(x => x.SalesOrderLineId == line.SalesOrderLineId && !x.IsDeleted);
                if (soLine == null) throw new Exception($"Sales order line not found for {line.Description}");
                var alreadyDelivered = deliveredByLine.GetValueOrDefault(line.SalesOrderLineId, 0);
                if (line.Quantity + alreadyDelivered > soLine.Quantity)
                    throw new Exception($"Delivered quantity for {line.Description} exceeds order quantity (remaining: {soLine.Quantity - alreadyDelivered})");
            }

            foreach (var line in delivery.Lines)
            {
                if (!line.ItemId.HasValue) continue;
                _dbContext.StockMovements.Add(new StockMovementEntity
                {
                    StockMovementId = Guid.NewGuid(),
                    MovementDate = delivery.DeliveryDate,
                    ItemId = line.ItemId.Value,
                    WarehouseId = GetDefaultWarehouseId(),
                    MovementType = "Sales Delivery",
                    SourceDocumentType = "DeliveryOrder",
                    SourceDocumentId = delivery.DeliveryOrderId,
                    QuantityIn = 0,
                    QuantityOut = line.Quantity,
                    UnitCost = line.UnitPrice,
                    Notes = $"Delivery {delivery.DeliveryNumber}",
                    CreatedBy = userId.ToString()
                });
            }

            delivery.Status = "Posted";
            delivery.UpdatedAt = DateTime.UtcNow;
            delivery.UpdatedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var delivery = await _dbContext.DeliveryOrders.FirstOrDefaultAsync(x => x.DeliveryOrderId == id && !x.IsDeleted);
            if (delivery == null) throw new Exception("Delivery order not found");
            if (delivery.Status != "Draft") throw new Exception("Only draft delivery order can be deleted");
            delivery.IsDeleted = true; delivery.DeletedAt = DateTime.UtcNow; delivery.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task ConvertToInvoice(Guid id, Guid userId)
        {
            var delivery = await _dbContext.DeliveryOrders
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .FirstOrDefaultAsync(x => x.DeliveryOrderId == id && !x.IsDeleted);
            if (delivery == null) throw new Exception("Delivery order not found");
            if (delivery.Status != "Posted") throw new Exception("Only posted delivery order can be converted to invoice");
            if (delivery.InvoiceId.HasValue) throw new Exception("Delivery order already converted to invoice");

            var invoice = new InvoiceEntity
            {
                InvoiceId = Guid.NewGuid(),
                InvoiceNumber = await GenerateInvoiceNumberAsync(),
                InvoiceType = "Sales",
                InvoiceDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(30),
                CustomerId = delivery.CustomerId,
                Status = "Draft",
                SubTotal = delivery.TotalAmount,
                TotalAmount = delivery.TotalAmount,
                Notes = $"Auto created from delivery {delivery.DeliveryNumber}",
                CreatedBy = userId.ToString()
            };
            foreach (var line in delivery.Lines)
            {
                invoice.Lines.Add(new InvoiceLineEntity
                {
                    InvoiceLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    LineTotal = line.LineTotal,
                    CreatedBy = userId.ToString()
                });
            }
            delivery.InvoiceId = invoice.InvoiceId;
            delivery.Status = "Invoiced";
            delivery.UpdatedAt = DateTime.UtcNow;
            delivery.UpdatedBy = userId.ToString();
            _dbContext.Invoices.Add(invoice);
            await _dbContext.SaveChangesAsync();
        }

        private Guid GetDefaultWarehouseId()
        {
            return _dbContext.Warehouses.Where(x => x.IsActive && !x.IsDeleted).OrderBy(x => x.WarehouseCode).Select(x => x.WarehouseId).FirstOrDefault();
        }

        private async Task<Dictionary<Guid, decimal>> GetDeliveredBySoLineAsync()
        {
            return await _dbContext.DeliveryOrderLines
                .Where(x => !x.IsDeleted && x.DeliveryOrder != null && !x.DeliveryOrder.IsDeleted && x.DeliveryOrder.Status == "Posted")
                .GroupBy(x => x.SalesOrderLineId)
                .Select(g => new { SalesOrderLineId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.SalesOrderLineId, x => x.Quantity);
        }

        private async Task<string> GenerateNumberAsync()
        {
            var last = await _dbContext.DeliveryOrders.Where(x => x.DeliveryNumber.StartsWith("DO-"))
                .OrderByDescending(x => x.DeliveryNumber).Select(x => x.DeliveryNumber).FirstOrDefaultAsync();
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[3..]) + 1;
            return $"DO-{next:D5}";
        }

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var last = await _dbContext.Invoices.Where(x => x.InvoiceNumber.StartsWith("SI-")).OrderByDescending(x => x.InvoiceNumber).Select(x => x.InvoiceNumber).FirstOrDefaultAsync();
            var next = last == null ? 1 : int.Parse(last[3..]) + 1;
            return $"SI-{next:D5}";
        }
    }
}
