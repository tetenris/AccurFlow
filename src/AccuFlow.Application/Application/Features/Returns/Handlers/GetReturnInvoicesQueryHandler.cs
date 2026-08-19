using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Returns.Helpers;
using AccuFlow.Application.Features.Returns.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Return;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Returns.Handlers
{
    public class GetReturnInvoicesQueryHandler : IRequestHandler<GetReturnInvoicesQuery, List<ReturnInvoiceOptionViewModel>>
    {
        private readonly IRepository<InvoiceEntity> _invoiceRepository;
        private readonly IRepository<GoodsReturnLineEntity> _goodsReturnLineRepository;

        public GetReturnInvoicesQueryHandler(
            IRepository<InvoiceEntity> invoiceRepository,
            IRepository<GoodsReturnLineEntity> goodsReturnLineRepository)
        {
            _invoiceRepository = invoiceRepository;
            _goodsReturnLineRepository = goodsReturnLineRepository;
        }

        public async Task<List<ReturnInvoiceOptionViewModel>> Handle(GetReturnInvoicesQuery request, CancellationToken cancellationToken)
        {
            var returnedByLine = await ReturnHelper.GetReturnedByInvoiceLineAsync(_goodsReturnLineRepository, cancellationToken);

            var invoices = await _invoiceRepository.Query()
                .Include(x => x.Customer)
                .Include(x => x.Supplier)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .Where(x => x.InvoiceType == request.ReturnType && x.Status == "Posted" && !x.IsDeleted)
                .OrderByDescending(x => x.InvoiceDate)
                .Select(x => new
                {
                    x.InvoiceId,
                    x.InvoiceNumber,
                    x.InvoiceDate,
                    x.TotalAmount,
                    PartnerName = x.Customer != null ? x.Customer.CustomerName : x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                    Lines = x.Lines.Select(l => new { l.InvoiceLineId, l.Quantity }).ToList()
                })
                .ToListAsync(cancellationToken);

            var result = new List<ReturnInvoiceOptionViewModel>();
            foreach (var inv in invoices)
            {
                var remaining = inv.Lines.Sum(l =>
                {
                    var returnedQty = returnedByLine.GetValueOrDefault(l.InvoiceLineId, 0);
                    return Math.Max(0, l.Quantity - returnedQty);
                });
                if (remaining <= 0) continue;
                result.Add(new ReturnInvoiceOptionViewModel
                {
                    InvoiceId = inv.InvoiceId,
                    InvoiceNumber = inv.InvoiceNumber,
                    PartnerName = inv.PartnerName,
                    InvoiceDate = inv.InvoiceDate,
                    TotalAmount = inv.TotalAmount
                });
            }
            return result;
        }
    }
}