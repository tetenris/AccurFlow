using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Payrolls.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Payroll;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Payrolls.Handlers
{
    public class GetPayrollByIdQueryHandler : IRequestHandler<GetPayrollByIdQuery, PayrollDetailViewModel?>
    {
        private readonly IRepository<PayrollEntity> _payrollRepository;

        public GetPayrollByIdQueryHandler(IRepository<PayrollEntity> payrollRepository)
        {
            _payrollRepository = payrollRepository;
        }

        public async Task<PayrollDetailViewModel?> Handle(GetPayrollByIdQuery request, CancellationToken cancellationToken)
        {
            return await _payrollRepository.Query()
                .Include(x => x.JournalEntry)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(l => l.Employee)
                .Where(x => x.PayrollId == request.PayrollId && !x.IsDeleted)
                .Select(x => new PayrollDetailViewModel
                {
                    PayrollId = x.PayrollId,
                    PayrollNumber = x.PayrollNumber,
                    PeriodMonth = x.PeriodMonth,
                    PeriodYear = x.PeriodYear,
                    PayrollDate = x.PayrollDate,
                    Status = x.Status,
                    TotalAllowances = x.TotalAllowances,
                    TotalDeductions = x.TotalDeductions,
                    TotalNetSalary = x.TotalNetSalary,
                    JournalNumber = x.JournalEntry != null ? x.JournalEntry.JournalNumber : null,
                    Lines = x.Lines.OrderBy(l => l.Employee.EmployeeCode).Select(l => new PayrollLineViewModel
                    {
                        PayrollLineId = l.PayrollLineId,
                        EmployeeId = l.EmployeeId,
                        EmployeeCode = l.Employee.EmployeeCode,
                        EmployeeName = l.Employee.FullName,
                        BasicSalary = l.BasicSalary,
                        Allowances = l.Allowances,
                        Deductions = l.Deductions,
                        NetSalary = l.NetSalary
                    }).ToList()
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}