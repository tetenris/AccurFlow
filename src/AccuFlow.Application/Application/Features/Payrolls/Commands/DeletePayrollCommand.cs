using MediatR;

namespace AccuFlow.Application.Features.Payrolls.Commands
{
    public record DeletePayrollCommand(Guid PayrollId, Guid UserId) : IRequest;
}