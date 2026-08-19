using AccuFlow.Models.Quotation;
using MediatR;

namespace AccuFlow.Application.Features.SalesQuotations.Queries
{
    public record GetQuotationByIdQuery(Guid Id) : IRequest<QuotationDetailViewModel?>;
}