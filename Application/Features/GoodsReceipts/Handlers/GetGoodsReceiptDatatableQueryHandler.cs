using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.GoodsReceipts.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.GoodsReceipt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.GoodsReceipts.Handlers
{
    public class GetGoodsReceiptDatatableQueryHandler : IRequestHandler<GetGoodsReceiptDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<GoodsReceiptEntity> _grnRepository;

        public GetGoodsReceiptDatatableQueryHandler(IRepository<GoodsReceiptEntity> grnRepository)
        {
            _grnRepository = grnRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetGoodsReceiptDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _grnRepository.Query()
                .Include(x => x.Supplier).Include(x => x.Warehouse).Include(x => x.PurchaseOrder)
                .Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(r.Status)) query = query.Where(x => x.Status == r.Status);
            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.GoodsReceiptNumber.ToLower().Contains(search)
                    || x.PurchaseOrder.PurchaseOrderNumber.ToLower().Contains(search)
                    || x.Supplier.SupplierName.ToLower().Contains(search));
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderByDescending(x => x.ReceiptDate)
                .Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new GoodsReceiptViewModel
                {
                    GoodsReceiptId = x.GoodsReceiptId,
                    GoodsReceiptNumber = x.GoodsReceiptNumber,
                    ReceiptDate = x.ReceiptDate,
                    PurchaseOrderNumber = x.PurchaseOrder.PurchaseOrderNumber,
                    SupplierName = x.Supplier.SupplierName,
                    WarehouseName = x.Warehouse.WarehouseName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}