using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.SalesQuotations.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Quotation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.SalesQuotations.Handlers
{
    public class GetQuotationByIdQueryHandler : IRequestHandler<GetQuotationByIdQuery, QuotationDetailViewModel?>
    {
        private readonly IRepository<SalesQuotationEntity> _quotationRepository;

        public GetQuotationByIdQueryHandler(IRepository<SalesQuotationEntity> quotationRepository)
        {
            _quotationRepository = quotationRepository;
        }

        public async Task<QuotationDetailViewModel?> Handle(GetQuotationByIdQuery request, CancellationToken cancellationToken)
        {
            return await _quotationRepository.Query()
                .Include(x => x.Customer)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.SalesQuotationId == request.Id && !x.IsDeleted)
                .Select(x => new QuotationDetailViewModel
                {
                    SalesQuotationId = x.SalesQuotationId,
                    QuotationNumber = x.QuotationNumber,
                    QuotationDate = x.QuotationDate,
                    ValidUntil = x.ValidUntil,
                    CustomerId = x.CustomerId,
                    CustomerName = x.Customer.CustomerName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount,
                    Notes = x.Notes,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanApprove = x.Status == "Draft",
                    Lines = x.Lines.Select(l => new QuotationLineViewModel
                    {
                        SalesQuotationLineId = l.SalesQuotationLineId,
                        ItemId = l.ItemId,
                        ItemCode = l.Item != null ? l.Item.ItemCode : string.Empty,
                        ItemName = l.Item != null ? l.Item.ItemName : string.Empty,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        DiscountAmount = l.DiscountAmount,
                        TaxAmount = l.TaxAmount,
                        LineTotal = l.LineTotal
                    }).ToList()
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}