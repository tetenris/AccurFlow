using MediatR;

namespace AccuFlow.Application.Features.Employees.Commands
{
    public record DeleteEmployeeCommand(Guid EmployeeId, Guid UserId) : IRequest;
}