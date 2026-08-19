using AccuFlow.Models.Quotation;
using MediatR;

namespace AccuFlow.Application.Features.SalesOrders.Queries
{
    public record GetOrderQuoteByIdQuery(Guid Id) : IRequest<QuotationDetailViewModel?>;
}