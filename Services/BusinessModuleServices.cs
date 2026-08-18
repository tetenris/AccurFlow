using AccuFlow.Entities.Context;
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
    public interface IInvoiceService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableInvoiceRequest request);
        Task<InvoiceDetailViewModel?> GetById(Guid id);
        Task Create(CreateInvoiceRequest request, Guid userId);
        Task Edit(UpdateInvoiceRequest request, Guid userId);
        Task Delete(Guid id, Guid userId);
        Task Post(Guid id, Guid userId);
        Task Cancel(Guid id, Guid userId);
    }

    public class InvoiceService : BaseService, IInvoiceService
    {
        private static readonly Guid AccountsReceivableAccountId = Guid.Parse("10000000-0000-0000-0000-000000000004");
        private static readonly Guid AccountsPayableAccountId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        private static readonly Guid TaxPayableAccountId = Guid.Parse("20000000-0000-0000-0000-000000000003");
        private static readonly Guid SalesRevenueAccountId = Guid.Parse("40000000-0000-0000-0000-000000000002");
        private static readonly Guid CostOfGoodsSoldAccountId = Guid.Parse("50000000-0000-0000-0000-000000000002");

        private readonly IJournalEntryService _journalEntryService;

        public InvoiceService(AppDbContext dbContext, IJournalEntryService journalEntryService) : base(dbContext)
        {
            _journalEntryService = journalEntryService;
        }

        public async Task<BaseDatatableResponse> Datatable(DataTableInvoiceRequest request)
        {
            var query = _dbContext.Invoices.Include(x => x.Customer).Include(x => x.Supplier).Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.InvoiceType)) query = query.Where(x => x.InvoiceType == request.InvoiceType);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            if (request.CustomerId.HasValue) query = query.Where(x => x.CustomerId == request.CustomerId);
            if (request.SupplierId.HasValue) query = query.Where(x => x.SupplierId == request.SupplierId);
            if (request.DateFrom.HasValue) query = query.Where(x => x.InvoiceDate >= request.DateFrom.Value.Date);
            if (request.DateTo.HasValue) query = query.Where(x => x.InvoiceDate <= request.DateTo.Value.Date);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.InvoiceNumber.ToLower().Contains(search));
            }

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.InvoiceDate).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new InvoiceViewModel
                {
                    InvoiceId = x.InvoiceId,
                    InvoiceNumber = x.InvoiceNumber,
                    InvoiceType = x.InvoiceType,
                    InvoiceDate = x.InvoiceDate,
                    DueDate = x.DueDate,
                    PartnerName = x.Customer != null ? x.Customer.CustomerName : x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                    Status = x.Status,
                    TotalAmount = x.TotalAmount,
                    PaidAmount = x.PaidAmount
                }).ToListAsync();

            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<InvoiceDetailViewModel?> GetById(Guid id)
        {
            return await _dbContext.Invoices
                .Include(x => x.Customer)
                .Include(x => x.Supplier)
                .Include(x => x.JournalEntry)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                    .ThenInclude(x => x.Item)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                    .ThenInclude(x => x.Account)
                .Where(x => x.InvoiceId == id && !x.IsDeleted)
                .Select(x => new InvoiceDetailViewModel
                {
                    InvoiceId = x.InvoiceId,
                    InvoiceNumber = x.InvoiceNumber,
                    InvoiceType = x.InvoiceType,
                    InvoiceDate = x.InvoiceDate,
                    DueDate = x.DueDate,
                    CustomerId = x.CustomerId,
                    SupplierId = x.SupplierId,
                    PartnerName = x.Customer != null ? x.Customer.CustomerName : x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                    Status = x.Status,
                    TotalAmount = x.TotalAmount,
                    PaidAmount = x.PaidAmount,
                    JournalId = x.JournalId,
                    JournalNumber = x.JournalEntry != null ? x.JournalEntry.JournalNumber : null,
                    Notes = x.Notes,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanPost = x.Status == "Draft",
                    CanCancel = x.Status != "Cancelled" && x.PaidAmount == 0,
                    Lines = x.Lines.Where(l => !l.IsDeleted).Select(l => new InvoiceLineViewModel
                    {
                        InvoiceLineId = l.InvoiceLineId,
                        ItemId = l.ItemId,
                        ItemName = l.Item != null ? l.Item.ItemName : null,
                        AccountId = l.AccountId,
                        AccountName = l.Account != null ? l.Account.AccountName : null,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        DiscountAmount = l.DiscountAmount,
                        TaxAmount = l.TaxAmount,
                        LineTotal = l.LineTotal
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task Create(CreateInvoiceRequest request, Guid userId)
        {
            if (request.InvoiceType == "Sales" && !request.CustomerId.HasValue) throw new Exception("Customer is required for sales invoice");
            if (request.InvoiceType == "Purchase" && !request.SupplierId.HasValue) throw new Exception("Supplier is required for purchase invoice");
            if (!request.Lines.Any()) throw new Exception("Invoice lines are required");

            var invoice = new InvoiceEntity
            {
                InvoiceId = Guid.NewGuid(),
                InvoiceNumber = await GenerateNumber(request.InvoiceType),
                InvoiceType = request.InvoiceType,
                InvoiceDate = request.InvoiceDate,
                DueDate = request.DueDate,
                CustomerId = request.CustomerId,
                SupplierId = request.SupplierId,
                Status = "Draft",
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            };

            foreach (var line in request.Lines)
            {
                var lineTotal = (line.Quantity * line.UnitPrice) - line.DiscountAmount + line.TaxAmount;
                invoice.Lines.Add(new InvoiceLineEntity
                {
                    InvoiceLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    AccountId = line.AccountId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    DiscountAmount = line.DiscountAmount,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                invoice.SubTotal += line.Quantity * line.UnitPrice;
                invoice.DiscountAmount += line.DiscountAmount;
                invoice.TaxAmount += line.TaxAmount;
                invoice.TotalAmount += lineTotal;
            }

            _dbContext.Invoices.Add(invoice);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Edit(UpdateInvoiceRequest request, Guid userId)
        {
            var invoice = await _dbContext.Invoices
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.InvoiceId == request.InvoiceId && !x.IsDeleted);
            if (invoice == null) throw new Exception("Invoice not found");
            if (invoice.Status != "Draft") throw new Exception("Only draft invoice can be edited");
            if (request.InvoiceType == "Sales" && !request.CustomerId.HasValue) throw new Exception("Customer is required for sales invoice");
            if (request.InvoiceType == "Purchase" && !request.SupplierId.HasValue) throw new Exception("Supplier is required for purchase invoice");
            if (!request.Lines.Any()) throw new Exception("Invoice lines are required");

            invoice.InvoiceType = request.InvoiceType;
            invoice.InvoiceDate = request.InvoiceDate;
            invoice.DueDate = request.DueDate;
            invoice.CustomerId = request.InvoiceType == "Sales" ? request.CustomerId : null;
            invoice.SupplierId = request.InvoiceType == "Purchase" ? request.SupplierId : null;
            invoice.Notes = request.Notes;
            invoice.SubTotal = 0;
            invoice.DiscountAmount = 0;
            invoice.TaxAmount = 0;
            invoice.TotalAmount = 0;
            invoice.UpdatedBy = userId.ToString();
            invoice.UpdatedAt = DateTime.UtcNow;

            foreach (var oldLine in invoice.Lines)
            {
                oldLine.IsDeleted = true;
                oldLine.DeletedBy = userId.ToString();
                oldLine.DeletedAt = DateTime.UtcNow;
            }

            foreach (var line in request.Lines)
            {
                var lineTotal = (line.Quantity * line.UnitPrice) - line.DiscountAmount + line.TaxAmount;
                invoice.Lines.Add(new InvoiceLineEntity
                {
                    InvoiceLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    AccountId = line.AccountId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    DiscountAmount = line.DiscountAmount,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                invoice.SubTotal += line.Quantity * line.UnitPrice;
                invoice.DiscountAmount += line.DiscountAmount;
                invoice.TaxAmount += line.TaxAmount;
                invoice.TotalAmount += lineTotal;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var invoice = await _dbContext.Invoices.Include(x => x.Lines).FirstOrDefaultAsync(x => x.InvoiceId == id && !x.IsDeleted);
            if (invoice == null) throw new Exception("Invoice not found");
            if (invoice.Status != "Draft") throw new Exception("Only draft invoice can be deleted");
            invoice.IsDeleted = true;
            invoice.DeletedBy = userId.ToString();
            invoice.DeletedAt = DateTime.UtcNow;
            foreach (var line in invoice.Lines)
            {
                line.IsDeleted = true;
                line.DeletedBy = userId.ToString();
                line.DeletedAt = DateTime.UtcNow;
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task Post(Guid id, Guid userId)
        {
            var invoice = await _dbContext.Invoices
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.InvoiceId == id && !x.IsDeleted);
            if (invoice == null) throw new Exception("Invoice not found");
            if (invoice.Status != "Draft") throw new Exception("Only draft invoice can be posted");
            if (invoice.JournalId.HasValue) throw new Exception("Invoice already has journal entry");

            var journalId = await CreateInvoiceJournal(invoice, userId);
            invoice.Status = "Posted";
            invoice.JournalId = journalId;
            invoice.UpdatedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task Cancel(Guid id, Guid userId)
        {
            var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(x => x.InvoiceId == id && !x.IsDeleted);
            if (invoice == null) throw new Exception("Invoice not found");
            if (invoice.PaidAmount > 0) throw new Exception("Paid invoice cannot be cancelled");
            invoice.Status = "Cancelled";
            invoice.UpdatedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        private async Task<string> GenerateNumber(string invoiceType)
        {
            var prefix = invoiceType == "Purchase" ? "PI" : "SI";
            var last = await _dbContext.Invoices.Where(x => x.InvoiceNumber.StartsWith(prefix + "-")).OrderByDescending(x => x.InvoiceNumber).FirstOrDefaultAsync();
            var next = last == null ? 1 : int.Parse(last.InvoiceNumber[(prefix.Length + 1)..]) + 1;
            return $"{prefix}-{next:D5}";
        }

        private async Task<Guid> CreateInvoiceJournal(InvoiceEntity invoice, Guid userId)
        {
            var lines = new List<Models.JournalEntry.JournalLineRequest>();
            var description = $"Auto journal for {invoice.InvoiceNumber}";

            if (invoice.InvoiceType == "Purchase")
            {
                lines.Add(new Models.JournalEntry.JournalLineRequest
                {
                    AccountId = CostOfGoodsSoldAccountId,
                    Description = description,
                    DebitAmount = invoice.SubTotal - invoice.DiscountAmount,
                    CreditAmount = 0
                });

                if (invoice.TaxAmount > 0)
                {
                    lines.Add(new Models.JournalEntry.JournalLineRequest
                    {
                        AccountId = TaxPayableAccountId,
                        Description = description,
                        DebitAmount = invoice.TaxAmount,
                        CreditAmount = 0
                    });
                }

                lines.Add(new Models.JournalEntry.JournalLineRequest
                {
                    AccountId = AccountsPayableAccountId,
                    Description = description,
                    DebitAmount = 0,
                    CreditAmount = invoice.TotalAmount
                });
            }
            else
            {
                lines.Add(new Models.JournalEntry.JournalLineRequest
                {
                    AccountId = AccountsReceivableAccountId,
                    Description = description,
                    DebitAmount = invoice.TotalAmount,
                    CreditAmount = 0
                });

                lines.Add(new Models.JournalEntry.JournalLineRequest
                {
                    AccountId = SalesRevenueAccountId,
                    Description = description,
                    DebitAmount = 0,
                    CreditAmount = invoice.SubTotal - invoice.DiscountAmount
                });

                if (invoice.TaxAmount > 0)
                {
                    lines.Add(new Models.JournalEntry.JournalLineRequest
                    {
                        AccountId = TaxPayableAccountId,
                        Description = description,
                        DebitAmount = 0,
                        CreditAmount = invoice.TaxAmount
                    });
                }
            }

            var journalId = await _journalEntryService.CreateAsync(new Models.JournalEntry.CreateJournalEntryRequest
            {
                JournalDate = invoice.InvoiceDate,
                Description = description,
                JournalLines = lines
            }, userId);

            await _journalEntryService.PostAsync(new Models.JournalEntry.PostJournalRequest
            {
                JournalId = journalId,
                PostedDate = invoice.InvoiceDate
            }, userId);

            return journalId;
        }
    }

    public interface IPaymentService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTablePaymentRequest request);
        Task<PaymentDetailViewModel?> GetById(Guid id);
        Task Create(CreatePaymentRequest request, Guid userId);
        Task Edit(UpdatePaymentRequest request, Guid userId);
        Task Delete(Guid id, Guid userId);
        Task Post(Guid id, Guid userId);
    }

    public class PaymentService : BaseService, IPaymentService
    {
        private static readonly Guid AccountsReceivableAccountId = Guid.Parse("10000000-0000-0000-0000-000000000004");
        private static readonly Guid AccountsPayableAccountId = Guid.Parse("20000000-0000-0000-0000-000000000002");

        private readonly IJournalEntryService _journalEntryService;

        public PaymentService(AppDbContext dbContext, IJournalEntryService journalEntryService) : base(dbContext)
        {
            _journalEntryService = journalEntryService;
        }

        public async Task<BaseDatatableResponse> Datatable(DataTablePaymentRequest request)
        {
            var query = _dbContext.Payments.Include(x => x.Customer).Include(x => x.Supplier).Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.PaymentType)) query = query.Where(x => x.PaymentType == request.PaymentType);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            if (request.DateFrom.HasValue) query = query.Where(x => x.PaymentDate >= request.DateFrom.Value.Date);
            if (request.DateTo.HasValue) query = query.Where(x => x.PaymentDate <= request.DateTo.Value.Date);
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.PaymentDate).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new PaymentViewModel
                {
                    PaymentId = x.PaymentId,
                    PaymentNumber = x.PaymentNumber,
                    PaymentType = x.PaymentType,
                    PaymentDate = x.PaymentDate,
                    PartnerName = x.Customer != null ? x.Customer.CustomerName : x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                    PaymentMethod = x.PaymentMethod,
                    Status = x.Status,
                    TotalAmount = x.TotalAmount
                }).ToListAsync();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<PaymentDetailViewModel?> GetById(Guid id)
        {
            return await _dbContext.Payments
                .Include(x => x.Customer)
                .Include(x => x.Supplier)
                .Include(x => x.CashBankAccount)
                .Include(x => x.JournalEntry)
                .Include(x => x.Allocations.Where(a => !a.IsDeleted))
                    .ThenInclude(x => x.Invoice)
                .Where(x => x.PaymentId == id && !x.IsDeleted)
                .Select(x => new PaymentDetailViewModel
                {
                    PaymentId = x.PaymentId,
                    PaymentNumber = x.PaymentNumber,
                    PaymentType = x.PaymentType,
                    PaymentDate = x.PaymentDate,
                    CustomerId = x.CustomerId,
                    SupplierId = x.SupplierId,
                    PartnerName = x.Customer != null ? x.Customer.CustomerName : x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                    CashBankAccountId = x.CashBankAccountId,
                    CashBankAccountName = x.CashBankAccount.AccountName,
                    PaymentMethod = x.PaymentMethod,
                    Status = x.Status,
                    TotalAmount = x.TotalAmount,
                    JournalId = x.JournalId,
                    JournalNumber = x.JournalEntry != null ? x.JournalEntry.JournalNumber : null,
                    ReferenceNumber = x.ReferenceNumber,
                    Notes = x.Notes,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanPost = x.Status == "Draft",
                    Allocations = x.Allocations.Where(a => !a.IsDeleted).Select(a => new PaymentAllocationViewModel
                    {
                        PaymentAllocationId = a.PaymentAllocationId,
                        InvoiceId = a.InvoiceId,
                        InvoiceNumber = a.Invoice.InvoiceNumber,
                        AllocatedAmount = a.AllocatedAmount
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task Create(CreatePaymentRequest request, Guid userId)
        {
            var payment = new PaymentEntity
            {
                PaymentId = Guid.NewGuid(),
                PaymentNumber = await GenerateNumber(request.PaymentType),
                PaymentType = request.PaymentType,
                PaymentDate = request.PaymentDate,
                CustomerId = request.CustomerId,
                SupplierId = request.SupplierId,
                CashBankAccountId = request.CashBankAccountId,
                PaymentMethod = request.PaymentMethod,
                ReferenceNumber = request.ReferenceNumber,
                Notes = request.Notes,
                Status = "Draft",
                CreatedBy = userId.ToString()
            };

            foreach (var allocation in request.Allocations)
            {
                payment.Allocations.Add(new PaymentAllocationEntity
                {
                    PaymentAllocationId = Guid.NewGuid(),
                    InvoiceId = allocation.InvoiceId,
                    AllocatedAmount = allocation.AllocatedAmount,
                    CreatedBy = userId.ToString()
                });
                payment.TotalAmount += allocation.AllocatedAmount;
            }

            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Edit(UpdatePaymentRequest request, Guid userId)
        {
            var payment = await _dbContext.Payments
                .Include(x => x.Allocations)
                .FirstOrDefaultAsync(x => x.PaymentId == request.PaymentId && !x.IsDeleted);
            if (payment == null) throw new Exception("Payment not found");
            if (payment.Status != "Draft") throw new Exception("Only draft payment can be edited");
            if (!request.Allocations.Any()) throw new Exception("Payment allocation is required");

            payment.PaymentType = request.PaymentType;
            payment.PaymentDate = request.PaymentDate;
            payment.CustomerId = request.PaymentType == "Receipt" ? request.CustomerId : null;
            payment.SupplierId = request.PaymentType == "Payment" ? request.SupplierId : null;
            payment.CashBankAccountId = request.CashBankAccountId;
            payment.PaymentMethod = request.PaymentMethod;
            payment.ReferenceNumber = request.ReferenceNumber;
            payment.Notes = request.Notes;
            payment.TotalAmount = 0;
            payment.UpdatedBy = userId.ToString();
            payment.UpdatedAt = DateTime.UtcNow;

            foreach (var oldAllocation in payment.Allocations)
            {
                oldAllocation.IsDeleted = true;
                oldAllocation.DeletedBy = userId.ToString();
                oldAllocation.DeletedAt = DateTime.UtcNow;
            }

            foreach (var allocation in request.Allocations)
            {
                payment.Allocations.Add(new PaymentAllocationEntity
                {
                    PaymentAllocationId = Guid.NewGuid(),
                    InvoiceId = allocation.InvoiceId,
                    AllocatedAmount = allocation.AllocatedAmount,
                    CreatedBy = userId.ToString()
                });
                payment.TotalAmount += allocation.AllocatedAmount;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var payment = await _dbContext.Payments.Include(x => x.Allocations).FirstOrDefaultAsync(x => x.PaymentId == id && !x.IsDeleted);
            if (payment == null) throw new Exception("Payment not found");
            if (payment.Status != "Draft") throw new Exception("Only draft payment can be deleted");
            payment.IsDeleted = true;
            payment.DeletedBy = userId.ToString();
            payment.DeletedAt = DateTime.UtcNow;
            foreach (var allocation in payment.Allocations)
            {
                allocation.IsDeleted = true;
                allocation.DeletedBy = userId.ToString();
                allocation.DeletedAt = DateTime.UtcNow;
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task Post(Guid id, Guid userId)
        {
            var payment = await _dbContext.Payments.Include(x => x.Allocations).FirstOrDefaultAsync(x => x.PaymentId == id && !x.IsDeleted);
            if (payment == null) throw new Exception("Payment not found");
            if (payment.Status != "Draft") throw new Exception("Only draft payment can be posted");
            if (!payment.Allocations.Any()) throw new Exception("Payment allocation is required");
            if (payment.JournalId.HasValue) throw new Exception("Payment already has journal entry");

            var journalId = await CreatePaymentJournal(payment, userId);
            foreach (var allocation in payment.Allocations)
            {
                var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(x => x.InvoiceId == allocation.InvoiceId && !x.IsDeleted);
                if (invoice == null) continue;
                invoice.PaidAmount += allocation.AllocatedAmount;
                invoice.Status = invoice.PaidAmount >= invoice.TotalAmount ? "Paid" : "PartiallyPaid";
            }
            payment.Status = "Posted";
            payment.JournalId = journalId;
            payment.UpdatedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        private async Task<string> GenerateNumber(string paymentType)
        {
            var prefix = paymentType == "Payment" ? "PAY" : "RCT";
            var last = await _dbContext.Payments.Where(x => x.PaymentNumber.StartsWith(prefix + "-")).OrderByDescending(x => x.PaymentNumber).FirstOrDefaultAsync();
            var next = last == null ? 1 : int.Parse(last.PaymentNumber[(prefix.Length + 1)..]) + 1;
            return $"{prefix}-{next:D5}";
        }

        private async Task<Guid> CreatePaymentJournal(PaymentEntity payment, Guid userId)
        {
            var description = $"Auto journal for {payment.PaymentNumber}";
            var lines = new List<Models.JournalEntry.JournalLineRequest>();

            if (payment.PaymentType == "Payment")
            {
                lines.Add(new Models.JournalEntry.JournalLineRequest
                {
                    AccountId = AccountsPayableAccountId,
                    Description = description,
                    DebitAmount = payment.TotalAmount,
                    CreditAmount = 0
                });
                lines.Add(new Models.JournalEntry.JournalLineRequest
                {
                    AccountId = payment.CashBankAccountId,
                    Description = description,
                    DebitAmount = 0,
                    CreditAmount = payment.TotalAmount
                });
            }
            else
            {
                lines.Add(new Models.JournalEntry.JournalLineRequest
                {
                    AccountId = payment.CashBankAccountId,
                    Description = description,
                    DebitAmount = payment.TotalAmount,
                    CreditAmount = 0
                });
                lines.Add(new Models.JournalEntry.JournalLineRequest
                {
                    AccountId = AccountsReceivableAccountId,
                    Description = description,
                    DebitAmount = 0,
                    CreditAmount = payment.TotalAmount
                });
            }

            var journalId = await _journalEntryService.CreateAsync(new Models.JournalEntry.CreateJournalEntryRequest
            {
                JournalDate = payment.PaymentDate,
                Description = description,
                JournalLines = lines
            }, userId);

            await _journalEntryService.PostAsync(new Models.JournalEntry.PostJournalRequest
            {
                JournalId = journalId,
                PostedDate = payment.PaymentDate
            }, userId);

            return journalId;
        }
    }

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

