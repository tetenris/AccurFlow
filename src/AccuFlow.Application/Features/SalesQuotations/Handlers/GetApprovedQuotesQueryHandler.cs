using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.SalesQuotations.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.SalesOrder;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.SalesQuotations.Handlers
{
    public class GetApprovedQuotesQueryHandler : IRequestHandler<GetApprovedQuotesQuery, List<QuoteOptionViewModel>>
    {
        private readonly IRepository<SalesQuotationEntity> _quotationRepository;

        public GetApprovedQuotesQueryHandler(IRepository<SalesQuotationEntity> quotationRepository)
        {
            _quotationRepository = quotationRepository;
        }

        public async Task<List<QuoteOptionViewModel>> Handle(GetApprovedQuotesQuery request, CancellationToken cancellationToken)
        {
            return await _quotationRepository.Query()
                .Include(x => x.Customer)
                .Where(x => x.Status == "Approved" && !x.IsDeleted)
                .OrderByDescending(x => x.QuotationDate)
                .Select(x => new QuoteOptionViewModel
                {
                    SalesQuotationId = x.SalesQuotationId,
                    QuotationNumber = x.QuotationNumber,
                    CustomerName = x.Customer.CustomerName,
                    QuotationDate = x.QuotationDate
                }).ToListAsync(cancellationToken);
        }
    }
}