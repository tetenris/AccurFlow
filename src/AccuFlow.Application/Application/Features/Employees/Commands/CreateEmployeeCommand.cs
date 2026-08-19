using AccuFlow.Models.Payroll;
using MediatR;

namespace AccuFlow.Application.Features.Employees.Commands
{
    public record CreateEmployeeCommand(EmployeeRequest Request, Guid UserId) : IRequest;
}