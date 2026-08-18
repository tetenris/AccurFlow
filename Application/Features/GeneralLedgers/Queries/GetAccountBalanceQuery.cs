using MediatR;

namespace AccuFlow.Application.Features.GeneralLedgers.Queries
{
    public record GetAccountBalanceQuery(Guid AccountId, DateTime? AsOfDate = null) : IRequest<decimal>;
}