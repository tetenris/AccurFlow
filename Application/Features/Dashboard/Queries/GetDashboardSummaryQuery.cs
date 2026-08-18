using MediatR;
using AccuFlow.Application.Features.Dashboard.Dtos;

namespace AccuFlow.Application.Features.Dashboard.Queries
{
    public record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;
}