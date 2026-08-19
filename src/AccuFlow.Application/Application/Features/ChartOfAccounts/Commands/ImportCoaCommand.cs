using MediatR;
using AccuFlow.Models.ChartOfAccount;
using Microsoft.AspNetCore.Http;

namespace AccuFlow.Application.Features.ChartOfAccounts.Commands
{
    public record ImportCoaCommand(IFormFile File, Guid UserId) : IRequest<ImportChartOfAccountResult>;
}