using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ReceivablePayables.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.ReceivablePayable;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ReceivablePayables.Handlers
{
    public class GetReceivablePayableQueryHandler : IRequestHandler<GetReceivablePayableQuery, List<ReceivablePayableViewModel>>
    {
        private readonly IRepository<InvoiceEntity> _invoiceRepository;

        public GetReceivablePayableQueryHandler(IRepository<InvoiceEntity> invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<List<ReceivablePayableViewModel>> Handle(GetReceivablePayableQuery request, CancellationToken cancellationToken)
        {
            var query = _invoiceRepository.Query().Include(x => x.Customer).Include(x => x.Supplier)
                .Where(x => !x.IsDeleted && x.Status != "Cancelled" && x.TotalAmount > x.PaidAmount);
            query = request.Request.ReportType == "AP" ? query.Where(x => x.InvoiceType == "Purchase") : query.Where(x => x.InvoiceType == "Sales");
            var invoices = await query.ToListAsync(cancellationToken);
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
                    Status = x.DueDate.Date < request.Request.AsOfDate.Date ? "Overdue" : "Open"
                };
            }).OrderBy(x => x.PartnerName).ThenBy(x => x.DueDate).ToList();
        }
    }
}