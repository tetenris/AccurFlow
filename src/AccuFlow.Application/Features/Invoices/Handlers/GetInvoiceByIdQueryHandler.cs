using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Invoices.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Invoice;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Invoices.Handlers
{
    public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDetailViewModel?>
    {
        private readonly IRepository<InvoiceEntity> _invoiceRepository;

        public GetInvoiceByIdQueryHandler(IRepository<InvoiceEntity> invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<InvoiceDetailViewModel?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
        {
            return await _invoiceRepository.Query()
                .Include(x => x.Customer)
                .Include(x => x.Supplier)
                .Include(x => x.JournalEntry)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                    .ThenInclude(x => x.Item)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                    .ThenInclude(x => x.Account)
                .Where(x => x.InvoiceId == request.InvoiceId && !x.IsDeleted)
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
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}