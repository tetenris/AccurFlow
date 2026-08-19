using MediatR;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.ChartOfAccount;

namespace AccuFlow.Application.Features.ChartOfAccounts.Queries
{
    public record GetCoaDatatableQuery(DataTableChartOfAccountRequest Request) : IRequest<BaseDatatableResponse>;
}