using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Employees.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Employees.Handlers
{
    public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand>
    {
        private readonly IRepository<EmployeeEntity> _employeeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateEmployeeCommandHandler(
            IRepository<EmployeeEntity> employeeRepository,
            IUnitOfWork unitOfWork)
        {
            _employeeRepository = employeeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var employee = await _employeeRepository.Query()
                .FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId && !x.IsDeleted, cancellationToken);
            if (employee == null) throw new Exception("Employee not found");
            if (string.IsNullOrWhiteSpace(r.FullName)) throw new Exception("Employee name is required");

            var exists = await _employeeRepository.Query()
                .AnyAsync(x => !x.IsDeleted && x.EmployeeId != request.EmployeeId && x.EmployeeCode == r.EmployeeCode.Trim(), cancellationToken);
            if (exists) throw new Exception("Employee code already exists");

            employee.EmployeeCode = r.EmployeeCode.Trim();
            employee.FullName = r.FullName.Trim();
            employee.Position = r.Position;
            employee.Department = r.Department;
            employee.HireDate = r.HireDate;
            employee.BasicSalary = r.BasicSalary;
            employee.BankAccountNumber = r.BankAccountNumber;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = request.UserId.ToString();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}