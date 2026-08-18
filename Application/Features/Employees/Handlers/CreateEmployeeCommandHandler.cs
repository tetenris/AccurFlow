using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Employees.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Employees.Handlers
{
    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand>
    {
        private readonly IRepository<EmployeeEntity> _employeeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateEmployeeCommandHandler(
            IRepository<EmployeeEntity> employeeRepository,
            IUnitOfWork unitOfWork)
        {
            _employeeRepository = employeeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            if (string.IsNullOrWhiteSpace(r.EmployeeCode)) throw new Exception("Employee code is required");
            if (string.IsNullOrWhiteSpace(r.FullName)) throw new Exception("Employee name is required");

            var exists = await _employeeRepository.Query()
                .AnyAsync(x => !x.IsDeleted && x.EmployeeCode == r.EmployeeCode.Trim(), cancellationToken);
            if (exists) throw new Exception("Employee code already exists");

            _employeeRepository.Add(new EmployeeEntity
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeCode = r.EmployeeCode.Trim(),
                FullName = r.FullName.Trim(),
                Position = r.Position,
                Department = r.Department,
                HireDate = r.HireDate,
                BasicSalary = r.BasicSalary,
                BankAccountNumber = r.BankAccountNumber,
                IsActive = true,
                CreatedBy = request.UserId.ToString()
            });
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}