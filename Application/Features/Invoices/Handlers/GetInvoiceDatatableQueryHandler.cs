using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Invoices.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Invoice;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Invoices.Handlers
{
    public class GetInvoiceDatatableQueryHandler : IRequestHandler<GetInvoiceDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<InvoiceEntity> _invoiceRepository;

        public GetInvoiceDatatableQueryHandler(IRepository<InvoiceEntity> invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetInvoiceDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _invoiceRepository.Query().Include(x => x.Customer).Include(x => x.Supplier).Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(r.InvoiceType)) query = query.Where(x => x.InvoiceType == r.InvoiceType);
            if (!string.IsNullOrWhiteSpace(r.Status)) query = query.Where(x => x.Status == r.Status);
            if (r.CustomerId.HasValue) query = query.Where(x => x.CustomerId == r.CustomerId);
            if (r.SupplierId.HasValue) query = query.Where(x => x.SupplierId == r.SupplierId);
            if (r.DateFrom.HasValue) query = query.Where(x => x.InvoiceDate >= r.DateFrom.Value.Date);
            if (r.DateTo.HasValue) query = query.Where(x => x.InvoiceDate <= r.DateTo.Value.Date);
            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.InvoiceNumber.ToLower().Contains(search));
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderByDescending(x => x.InvoiceDate).Skip((r.Page - 1) * r.Size).Take(r.Size)
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
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}