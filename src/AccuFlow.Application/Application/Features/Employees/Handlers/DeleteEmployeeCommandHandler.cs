using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Employees.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Employees.Handlers
{
    public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand>
    {
        private readonly IRepository<EmployeeEntity> _employeeRepository;
        private readonly IRepository<PayrollLineEntity> _payrollLineRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteEmployeeCommandHandler(
            IRepository<EmployeeEntity> employeeRepository,
            IRepository<PayrollLineEntity> payrollLineRepository,
            IUnitOfWork unitOfWork)
        {
            _employeeRepository = employeeRepository;
            _payrollLineRepository = payrollLineRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.Query()
                .FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId && !x.IsDeleted, cancellationToken);
            if (employee == null) throw new Exception("Employee not found");

            var usedInPayroll = await _payrollLineRepository.Query()
                .AnyAsync(x => x.EmployeeId == request.EmployeeId, cancellationToken);
            if (usedInPayroll) throw new Exception("Cannot delete employee that already has payroll records");

            employee.IsDeleted = true;
            employee.DeletedAt = DateTime.UtcNow;
            employee.DeletedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}