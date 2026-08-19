using MediatR;

namespace AccuFlow.Application.Features.ChartOfAccounts.Queries
{
    public record ValidateCoaCodeQuery(string Code, Guid? ExcludeId) : IRequest<bool>;
}