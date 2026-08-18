using MediatR;
using AccuFlow.Models.ChartOfAccount;

namespace AccuFlow.Application.Features.ChartOfAccounts.Queries
{
    public record GetCoaParentAccountsQuery(string AccountType) : IRequest<List<ChartOfAccountViewModel>>;
}