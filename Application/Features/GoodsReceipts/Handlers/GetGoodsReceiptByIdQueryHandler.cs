using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.GoodsReceipts.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.GoodsReceipt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.GoodsReceipts.Handlers
{
    public class GetGoodsReceiptByIdQueryHandler : IRequestHandler<GetGoodsReceiptByIdQuery, GoodsReceiptDetailViewModel?>
    {
        private readonly IRepository<GoodsReceiptEntity> _grnRepository;

        public GetGoodsReceiptByIdQueryHandler(IRepository<GoodsReceiptEntity> grnRepository)
        {
            _grnRepository = grnRepository;
        }

        public async Task<GoodsReceiptDetailViewModel?> Handle(GetGoodsReceiptByIdQuery request, CancellationToken cancellationToken)
        {
            return await _grnRepository.Query()
                .Include(x => x.Supplier).Include(x => x.Warehouse).Include(x => x.PurchaseOrder).Include(x => x.JournalEntry)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.GoodsReceiptId == request.Id && !x.IsDeleted)
                .Select(x => new GoodsReceiptDetailViewModel
                {
                    GoodsReceiptId = x.GoodsReceiptId,
                    GoodsReceiptNumber = x.GoodsReceiptNumber,
                    ReceiptDate = x.ReceiptDate,
                    PurchaseOrderId = x.PurchaseOrderId,
                    PurchaseOrderNumber = x.PurchaseOrder.PurchaseOrderNumber,
                    SupplierName = x.Supplier.SupplierName,
                    WarehouseName = x.Warehouse.WarehouseName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount,
                    Notes = x.Notes,
                    JournalNumber = x.JournalEntry != null ? x.JournalEntry.JournalNumber : null,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanPost = x.Status == "Draft",
                    Lines = x.Lines.Select(l => new GoodsReceiptLineViewModel
                    {
                        GoodsReceiptLineId = l.GoodsReceiptLineId,
                        PurchaseOrderLineId = l.PurchaseOrderLineId,
                        ItemId = l.ItemId,
                        ItemCode = l.Item != null ? l.Item.ItemCode : string.Empty,
                        ItemName = l.Item != null ? l.Item.ItemName : string.Empty,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        TaxAmount = l.TaxAmount,
                        LineTotal = l.LineTotal
                    }).ToList()
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}