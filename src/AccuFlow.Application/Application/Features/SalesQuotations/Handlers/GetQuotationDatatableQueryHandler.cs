using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.SalesQuotations.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Quotation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.SalesQuotations.Handlers
{
    public class GetQuotationDatatableQueryHandler : IRequestHandler<GetQuotationDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<SalesQuotationEntity> _quotationRepository;

        public GetQuotationDatatableQueryHandler(IRepository<SalesQuotationEntity> quotationRepository)
        {
            _quotationRepository = quotationRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetQuotationDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _quotationRepository.Query().Include(x => x.Customer).Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(r.Status)) query = query.Where(x => x.Status == r.Status);
            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.QuotationNumber.ToLower().Contains(search) || x.Customer.CustomerName.ToLower().Contains(search));
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderByDescending(x => x.QuotationDate).Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new QuotationViewModel
                {
                    SalesQuotationId = x.SalesQuotationId,
                    QuotationNumber = x.QuotationNumber,
                    QuotationDate = x.QuotationDate,
                    ValidUntil = x.ValidUntil,
                    CustomerId = x.CustomerId,
                    CustomerName = x.Customer.CustomerName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}