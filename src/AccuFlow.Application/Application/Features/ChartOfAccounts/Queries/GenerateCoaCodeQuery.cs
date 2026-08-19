using MediatR;

namespace AccuFlow.Application.Features.ChartOfAccounts.Queries
{
    public record GenerateCoaCodeQuery(Guid? ParentId, string AccountType) : IRequest<string>;
}