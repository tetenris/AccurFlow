using MediatR;
using AccuFlow.Models.ChartOfAccount;

namespace AccuFlow.Application.Features.ChartOfAccounts.Commands
{
    public record CreateCoaCommand(CreateChartOfAccountRequest Request, Guid UserId) : IRequest;
}