using MediatR;
using AccuFlow.Models.ChartOfAccount;

namespace AccuFlow.Application.Features.ChartOfAccounts.Queries
{
    public record GetCoaHierarchyQuery : IRequest<List<ChartOfAccountViewModel>>;
}