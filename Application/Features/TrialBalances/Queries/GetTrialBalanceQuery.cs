using AccuFlow.Models.TrialBalance;
using MediatR;

namespace AccuFlow.Application.Features.TrialBalances.Queries
{
    public record GetTrialBalanceQuery(GetTrialBalanceRequest Request) : IRequest<TrialBalanceViewModel>;
}