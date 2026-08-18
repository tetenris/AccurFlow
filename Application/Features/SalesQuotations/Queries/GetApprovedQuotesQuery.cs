using AccuFlow.Models.SalesOrder;
using MediatR;

namespace AccuFlow.Application.Features.SalesQuotations.Queries
{
    public record GetApprovedQuotesQuery : IRequest<List<QuoteOptionViewModel>>;
}