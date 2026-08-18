using AccuFlow.Models.TrialBalance;
using MediatR;

namespace AccuFlow.Application.Features.TrialBalances.Queries
{
    public record ExportTrialBalanceQuery(GetTrialBalanceRequest Request) : IRequest<byte[]>;
}