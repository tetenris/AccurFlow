using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.GoodsReceipts.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.GoodsReceipt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.GoodsReceipts.Handlers
{
    public class GetGoodsReceiptPurchaseOrdersQueryHandler : IRequestHandler<GetGoodsReceiptPurchaseOrdersQuery, List<PurchaseOrderOptionViewModel>>
    {
        private readonly IRepository<PurchaseOrderEntity> _poRepository;
        private readonly IRepository<GoodsReceiptLineEntity> _grnLineRepository;

        public GetGoodsReceiptPurchaseOrdersQueryHandler(
            IRepository<PurchaseOrderEntity> poRepository,
            IRepository<GoodsReceiptLineEntity> grnLineRepository)
        {
            _poRepository = poRepository;
            _grnLineRepository = grnLineRepository;
        }

        public async Task<List<PurchaseOrderOptionViewModel>> Handle(GetGoodsReceiptPurchaseOrdersQuery request, CancellationToken cancellationToken)
        {
            var receivedByLine = await _grnLineRepository.Query()
                .Where(x => !x.IsDeleted && x.GoodsReceipt != null && !x.GoodsReceipt.IsDeleted && x.GoodsReceipt.Status == "Posted")
                .GroupBy(x => x.PurchaseOrderLineId)
                .Select(g => new { PurchaseOrderLineId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.PurchaseOrderLineId, x => x.Quantity, cancellationToken);

            var pos = await _poRepository.Query()
                .Include(x => x.Supplier)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .Where(x => x.Status == "Approved" && !x.IsDeleted)
                .OrderByDescending(x => x.OrderDate)
                .Select(x => new
                {
                    x.PurchaseOrderId,
                    x.PurchaseOrderNumber,
                    x.Supplier.SupplierName,
                    x.OrderDate,
                    x.TotalAmount,
                    Lines = x.Lines.Select(l => new { l.PurchaseOrderLineId, l.Quantity }).ToList()
                })
                .ToListAsync(cancellationToken);

            var result = new List<PurchaseOrderOptionViewModel>();
            foreach (var po in pos)
            {
                var remaining = po.Lines.Sum(l =>
                {
                    var receivedQty = receivedByLine.GetValueOrDefault(l.PurchaseOrderLineId, 0);
                    return Math.Max(0, l.Quantity - receivedQty);
                });
                if (remaining <= 0) continue;
                result.Add(new PurchaseOrderOptionViewModel
                {
                    PurchaseOrderId = po.PurchaseOrderId,
                    PurchaseOrderNumber = po.PurchaseOrderNumber,
                    SupplierName = po.SupplierName,
                    OrderDate = po.OrderDate,
                    TotalAmount = po.TotalAmount
                });
            }
            return result;
        }
    }
}