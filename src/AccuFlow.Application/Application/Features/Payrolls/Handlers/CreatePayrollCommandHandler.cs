using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Payrolls.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Payrolls.Handlers
{
    public class CreatePayrollCommandHandler : IRequestHandler<CreatePayrollCommand>
    {
        private readonly IRepository<PayrollEntity> _payrollRepository;
        private readonly IRepository<EmployeeEntity> _employeeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePayrollCommandHandler(
            IRepository<PayrollEntity> payrollRepository,
            IRepository<EmployeeEntity> employeeRepository,
            IUnitOfWork unitOfWork)
        {
            _payrollRepository = payrollRepository;
            _employeeRepository = employeeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreatePayrollCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;
            if (r.PeriodMonth < 1 || r.PeriodMonth > 12) throw new Exception("Invalid period month");
            if (r.PeriodYear < 2000) throw new Exception("Invalid period year");

            var exists = await _payrollRepository.Query()
                .AnyAsync(x => !x.IsDeleted && x.PeriodMonth == r.PeriodMonth && x.PeriodYear == r.PeriodYear, cancellationToken);
            if (exists) throw new Exception($"Payroll for {r.PeriodMonth}/{r.PeriodYear} already exists");

            var employees = await _employeeRepository.Query()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.EmployeeCode)
                .ToListAsync(cancellationToken);
            if (employees.Count == 0) throw new Exception("No active employees found");

            var payroll = new PayrollEntity
            {
                PayrollId = Guid.NewGuid(),
                PayrollNumber = await GenerateNumberAsync(r.PeriodMonth, r.PeriodYear, cancellationToken),
                PeriodMonth = r.PeriodMonth,
                PeriodYear = r.PeriodYear,
                PayrollDate = r.PayrollDate,
                Status = "Draft",
                Notes = r.Notes,
                CreatedBy = userId.ToString()
            };

            foreach (var employee in employees)
            {
                var basicSalary = employee.BasicSalary;
                payroll.Lines.Add(new PayrollLineEntity
                {
                    PayrollLineId = Guid.NewGuid(),
                    EmployeeId = employee.EmployeeId,
                    BasicSalary = basicSalary,
                    Allowances = 0,
                    Deductions = 0,
                    NetSalary = basicSalary,
                    CreatedBy = userId.ToString()
                });
                payroll.TotalNetSalary += basicSalary;
            }

            _payrollRepository.Add(payroll);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<string> GenerateNumberAsync(int month, int year, CancellationToken cancellationToken)
        {
            var prefix = $"PR-{year}{month:D2}-";
            var last = await _payrollRepository.Query()
                .Where(x => x.PayrollNumber.StartsWith(prefix))
                .OrderByDescending(x => x.PayrollNumber)
                .Select(x => x.PayrollNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[prefix.Length..]) + 1;
            return $"{prefix}{next:D4}";
        }
    }
}