using MediatR;

namespace AccuFlow.Application.Features.ChartOfAccounts.Queries
{
    public record DownloadCoaTemplateQuery : IRequest<byte[]>;
}