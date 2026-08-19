using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.PurchaseRequests.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.PurchaseRequest;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.PurchaseRequests.Handlers
{
    public class GetPurchaseRequestDatatableQueryHandler : IRequestHandler<GetPurchaseRequestDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<PurchaseRequestEntity> _prRepository;

        public GetPurchaseRequestDatatableQueryHandler(IRepository<PurchaseRequestEntity> prRepository)
        {
            _prRepository = prRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetPurchaseRequestDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _prRepository.Query().Include(x => x.PurchaseOrder).Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(r.Status)) query = query.Where(x => x.Status == r.Status);
            if (r.DateFrom.HasValue) query = query.Where(x => x.RequestDate >= r.DateFrom.Value.Date);
            if (r.DateTo.HasValue) query = query.Where(x => x.RequestDate <= r.DateTo.Value.Date);
            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.PurchaseRequestNumber.ToLower().Contains(search) || x.RequestedBy.ToLower().Contains(search) || (x.Department != null && x.Department.ToLower().Contains(search)));
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderByDescending(x => x.RequestDate).Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new PurchaseRequestViewModel
                {
                    PurchaseRequestId = x.PurchaseRequestId,
                    PurchaseRequestNumber = x.PurchaseRequestNumber,
                    RequestDate = x.RequestDate,
                    RequiredDate = x.RequiredDate,
                    RequestedBy = x.RequestedBy,
                    Department = x.Department,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount,
                    PurchaseOrderNumber = x.PurchaseOrder != null ? x.PurchaseOrder.PurchaseOrderNumber : null
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}