using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.AgingReports.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.AgingReport;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.AgingReports.Handlers
{
    public class GetAgingReportQueryHandler : IRequestHandler<GetAgingReportQuery, List<AgingReportViewModel>>
    {
        private readonly IRepository<InvoiceEntity> _invoiceRepository;

        public GetAgingReportQueryHandler(IRepository<InvoiceEntity> invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<List<AgingReportViewModel>> Handle(GetAgingReportQuery request, CancellationToken cancellationToken)
        {
            var query = _invoiceRepository.Query().Include(x => x.Customer).Include(x => x.Supplier)
                .Where(x => !x.IsDeleted && x.Status != "Cancelled" && x.TotalAmount > x.PaidAmount);
            query = request.Request.AgingType == "AP" ? query.Where(x => x.InvoiceType == "Purchase") : query.Where(x => x.InvoiceType == "Sales");
            var invoices = await query.ToListAsync(cancellationToken);
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
                    var age = (request.Request.AsOfDate.Date - invoice.DueDate.Date).Days;
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
}