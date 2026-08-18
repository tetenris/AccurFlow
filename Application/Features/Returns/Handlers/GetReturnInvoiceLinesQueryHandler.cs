using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Returns.Helpers;
using AccuFlow.Application.Features.Returns.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Return;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Returns.Handlers
{
    public class GetReturnInvoiceLinesQueryHandler : IRequestHandler<GetReturnInvoiceLinesQuery, ReturnInvoiceViewModel>
    {
        private readonly IRepository<InvoiceEntity> _invoiceRepository;
        private readonly IRepository<GoodsReturnLineEntity> _goodsReturnLineRepository;

        public GetReturnInvoiceLinesQueryHandler(
            IRepository<InvoiceEntity> invoiceRepository,
            IRepository<GoodsReturnLineEntity> goodsReturnLineRepository)
        {
            _invoiceRepository = invoiceRepository;
            _goodsReturnLineRepository = goodsReturnLineRepository;
        }

        public async Task<ReturnInvoiceViewModel> Handle(GetReturnInvoiceLinesQuery request, CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.Query()
                .Include(x => x.Customer)
                .Include(x => x.Supplier)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .FirstOrDefaultAsync(x => x.InvoiceId == request.InvoiceId && !x.IsDeleted, cancellationToken);
            if (invoice == null) throw new Exception("Invoice not found");

            var returnedByLine = await ReturnHelper.GetReturnedByInvoiceLineAsync(_goodsReturnLineRepository, cancellationToken);
            var lines = invoice.Lines.Where(l => !l.IsDeleted).Select(l =>
            {
                var returnedQty = returnedByLine.GetValueOrDefault(l.InvoiceLineId, 0);
                return new ReturnInvoiceLineViewModel
                {
                    InvoiceLineId = l.InvoiceLineId,
                    ItemId = l.ItemId,
                    ItemCode = l.Item != null ? l.Item.ItemCode : string.Empty,
                    ItemName = l.Item != null ? l.Item.ItemName : string.Empty,
                    Description = l.Description,
                    Quantity = l.Quantity,
                    ReturnedQuantity = returnedQty,
                    RemainingQuantity = l.Quantity - returnedQty,
                    UnitPrice = l.UnitPrice,
                    TaxAmount = l.TaxAmount
                };
            })
            .Where(x => x.RemainingQuantity > 0)
            .ToList();

            return new ReturnInvoiceViewModel
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceType = invoice.InvoiceType,
                CustomerId = invoice.CustomerId,
                SupplierId = invoice.SupplierId,
                PartnerName = invoice.Customer != null ? invoice.Customer.CustomerName : invoice.Supplier != null ? invoice.Supplier.SupplierName : string.Empty,
                InvoiceDate = invoice.InvoiceDate,
                Lines = lines
            };
        }
    }
}