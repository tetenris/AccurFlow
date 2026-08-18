using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.GoodsReceipts.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.GoodsReceipt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.GoodsReceipts.Handlers
{
    public class GetGoodsReceiptPurchaseOrderLinesQueryHandler : IRequestHandler<GetGoodsReceiptPurchaseOrderLinesQuery, PurchaseOrderReceiptViewModel>
    {
        private readonly IRepository<PurchaseOrderEntity> _poRepository;
        private readonly IRepository<GoodsReceiptLineEntity> _grnLineRepository;

        public GetGoodsReceiptPurchaseOrderLinesQueryHandler(
            IRepository<PurchaseOrderEntity> poRepository,
            IRepository<GoodsReceiptLineEntity> grnLineRepository)
        {
            _poRepository = poRepository;
            _grnLineRepository = grnLineRepository;
        }

        public async Task<PurchaseOrderReceiptViewModel> Handle(GetGoodsReceiptPurchaseOrderLinesQuery request, CancellationToken cancellationToken)
        {
            var po = await _poRepository.Query()
                .Include(x => x.Supplier)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .FirstOrDefaultAsync(x => x.PurchaseOrderId == request.PurchaseOrderId && !x.IsDeleted, cancellationToken);
            if (po == null) throw new Exception("Purchase order not found");

            var receivedByLine = await _grnLineRepository.Query()
                .Where(x => !x.IsDeleted && x.GoodsReceipt != null && !x.GoodsReceipt.IsDeleted && x.GoodsReceipt.Status == "Posted")
                .GroupBy(x => x.PurchaseOrderLineId)
                .Select(g => new { PurchaseOrderLineId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.PurchaseOrderLineId, x => x.Quantity, cancellationToken);

            var lines = po.Lines.Where(l => !l.IsDeleted).Select(l =>
            {
                var receivedQty = receivedByLine.GetValueOrDefault(l.PurchaseOrderLineId, 0);
                return new PurchaseOrderReceiptLineViewModel
                {
                    PurchaseOrderLineId = l.PurchaseOrderLineId,
                    ItemId = l.ItemId,
                    ItemCode = l.Item != null ? l.Item.ItemCode : string.Empty,
                    ItemName = l.Item != null ? l.Item.ItemName : string.Empty,
                    Description = l.Description,
                    Quantity = l.Quantity,
                    ReceivedQuantity = receivedQty,
                    RemainingQuantity = l.Quantity - receivedQty,
                    UnitPrice = l.UnitPrice,
                    TaxAmount = l.TaxAmount
                };
            }).Where(x => x.RemainingQuantity > 0).ToList();

            return new PurchaseOrderReceiptViewModel
            {
                PurchaseOrderId = po.PurchaseOrderId,
                PurchaseOrderNumber = po.PurchaseOrderNumber,
                SupplierName = po.Supplier.SupplierName,
                OrderDate = po.OrderDate,
                Lines = lines
            };
        }
    }
}