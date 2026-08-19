using MediatR;

namespace AccuFlow.Application.Features.ChartOfAccounts.Queries
{
    public record ExportCoaQuery(string? AccountType, bool? IsActive) : IRequest<byte[]>;
}