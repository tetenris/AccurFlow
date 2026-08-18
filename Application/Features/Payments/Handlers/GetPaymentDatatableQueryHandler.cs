using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Payments.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Payment;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Payments.Handlers
{
    public class GetPaymentDatatableQueryHandler : IRequestHandler<GetPaymentDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<PaymentEntity> _paymentRepository;

        public GetPaymentDatatableQueryHandler(IRepository<PaymentEntity> paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetPaymentDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _paymentRepository.Query().Include(x => x.Customer).Include(x => x.Supplier).Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(r.PaymentType)) query = query.Where(x => x.PaymentType == r.PaymentType);
            if (!string.IsNullOrWhiteSpace(r.Status)) query = query.Where(x => x.Status == r.Status);
            if (r.DateFrom.HasValue) query = query.Where(x => x.PaymentDate >= r.DateFrom.Value.Date);
            if (r.DateTo.HasValue) query = query.Where(x => x.PaymentDate <= r.DateTo.Value.Date);
            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderByDescending(x => x.PaymentDate).Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new PaymentViewModel
                {
                    PaymentId = x.PaymentId,
                    PaymentNumber = x.PaymentNumber,
                    PaymentType = x.PaymentType,
                    PaymentDate = x.PaymentDate,
                    PartnerName = x.Customer != null ? x.Customer.CustomerName : x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                    PaymentMethod = x.PaymentMethod,
                    Status = x.Status,
                    TotalAmount = x.TotalAmount
                }).ToListAsync(cancellationToken);
            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}