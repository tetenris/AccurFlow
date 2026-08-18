using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Taxes.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Tax;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Taxes.Handlers
{
    public class GetTaxByIdQueryHandler : IRequestHandler<GetTaxByIdQuery, TaxViewModel?>
    {
        private readonly IRepository<TaxEntity> _taxRepository;

        public GetTaxByIdQueryHandler(IRepository<TaxEntity> taxRepository)
        {
            _taxRepository = taxRepository;
        }

        public async Task<TaxViewModel?> Handle(GetTaxByIdQuery request, CancellationToken cancellationToken)
        {
            return await _taxRepository.Query()
                .Where(x => x.TaxId == request.TaxId && !x.IsDeleted)
                .Select(x => new TaxViewModel
                {
                    TaxId = x.TaxId,
                    TaxCode = x.TaxCode,
                    TaxName = x.TaxName,
                    Rate = x.Rate,
                    TaxType = x.TaxType,
                    IsActive = x.IsActive
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}