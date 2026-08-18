using AccuFlow.Models.Payroll;
using MediatR;

namespace AccuFlow.Application.Features.Payrolls.Commands
{
    public record CreatePayrollCommand(CreatePayrollRequest Request, Guid UserId) : IRequest;
}