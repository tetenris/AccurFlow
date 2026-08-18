using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Taxes.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Tax;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Taxes.Handlers
{
    public class GetVatReportQueryHandler : IRequestHandler<GetVatReportQuery, BaseDatatableResponse>
    {
        private readonly IRepository<InvoiceEntity> _invoiceRepository;

        public GetVatReportQueryHandler(IRepository<InvoiceEntity> invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetVatReportQuery request, CancellationToken cancellationToken)
        {
            var invoices = await _invoiceRepository.Query()
                .Include(x => x.Customer).Include(x => x.Supplier)
                .Where(x => !x.IsDeleted && x.Status == "Posted"
                    && x.TaxAmount != 0
                    && x.InvoiceDate.Date >= request.Request.FromDate.Date && x.InvoiceDate.Date <= request.Request.ToDate.Date)
                .ToListAsync(cancellationToken);
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
    }
}