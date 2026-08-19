using MediatR;

namespace AccuFlow.Application.Features.Payrolls.Commands
{
    public record PostPayrollCommand(Guid PayrollId, Guid UserId) : IRequest;
}