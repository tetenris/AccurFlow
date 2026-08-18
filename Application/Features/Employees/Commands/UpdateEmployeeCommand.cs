using AccuFlow.Models.Payroll;
using MediatR;

namespace AccuFlow.Application.Features.Employees.Commands
{
    public record UpdateEmployeeCommand(Guid EmployeeId, EmployeeRequest Request, Guid UserId) : IRequest;
}