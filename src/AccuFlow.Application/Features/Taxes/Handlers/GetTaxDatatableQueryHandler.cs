using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Taxes.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Tax;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Taxes.Handlers
{
    public class GetTaxDatatableQueryHandler : IRequestHandler<GetTaxDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<TaxEntity> _taxRepository;

        public GetTaxDatatableQueryHandler(IRepository<TaxEntity> taxRepository)
        {
            _taxRepository = taxRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetTaxDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _taxRepository.Query().Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.TaxCode.ToLower().Contains(search) || x.TaxName.ToLower().Contains(search));
            }
            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderBy(x => x.TaxCode).Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new TaxViewModel
                {
                    TaxId = x.TaxId,
                    TaxCode = x.TaxCode,
                    TaxName = x.TaxName,
                    Rate = x.Rate,
                    TaxType = x.TaxType,
                    IsActive = x.IsActive
                }).ToListAsync(cancellationToken);
            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}