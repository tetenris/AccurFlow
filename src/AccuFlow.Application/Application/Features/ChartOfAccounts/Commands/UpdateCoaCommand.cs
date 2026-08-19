using MediatR;
using AccuFlow.Models.ChartOfAccount;

namespace AccuFlow.Application.Features.ChartOfAccounts.Commands
{
    public record UpdateCoaCommand(UpdateChartOfAccountRequest Request, Guid UserId) : IRequest;
}