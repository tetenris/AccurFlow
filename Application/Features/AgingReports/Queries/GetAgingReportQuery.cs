using AccuFlow.Models.AgingReport;
using MediatR;

namespace AccuFlow.Application.Features.AgingReports.Queries
{
    public record GetAgingReportQuery(AgingReportRequest Request) : IRequest<List<AgingReportViewModel>>;
}