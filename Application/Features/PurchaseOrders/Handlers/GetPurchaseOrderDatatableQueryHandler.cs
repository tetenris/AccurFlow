using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.PurchaseOrders.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.PurchaseOrder;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.PurchaseOrders.Handlers
{
    public class GetPurchaseOrderDatatableQueryHandler : IRequestHandler<GetPurchaseOrderDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<PurchaseOrderEntity> _poRepository;

        public GetPurchaseOrderDatatableQueryHandler(IRepository<PurchaseOrderEntity> poRepository)
        {
            _poRepository = poRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetPurchaseOrderDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _poRepository.Query().Include(x => x.Supplier).Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(r.Status)) query = query.Where(x => x.Status == r.Status);
            if (r.SupplierId.HasValue) query = query.Where(x => x.SupplierId == r.SupplierId);

            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderByDescending(x => x.OrderDate).Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new PurchaseOrderViewModel
                {
                    PurchaseOrderId = x.PurchaseOrderId,
                    PurchaseOrderNumber = x.PurchaseOrderNumber,
                    OrderDate = x.OrderDate,
                    ExpectedDate = x.ExpectedDate,
                    SupplierName = x.Supplier.SupplierName,
                    Status = x.Status,
                    TotalAmount = x.TotalAmount
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}