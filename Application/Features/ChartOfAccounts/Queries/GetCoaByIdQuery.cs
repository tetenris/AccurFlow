using MediatR;
using AccuFlow.Models.ChartOfAccount;

namespace AccuFlow.Application.Features.ChartOfAccounts.Queries
{
    public record GetCoaByIdQuery(Guid AccountId) : IRequest<ChartOfAccountViewModel?>;
}