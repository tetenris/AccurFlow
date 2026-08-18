using AccuFlow.Infrastructure.Persistence;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.JournalEntry;
using AccuFlow.Models.Payroll;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface IPayrollService : IBaseService
    {
        Task<BaseDatatableResponse> EmployeeDatatable(BaseDatatableRequest request);
        Task CreateEmployee(EmployeeRequest request, Guid userId);
        Task UpdateEmployee(Guid employeeId, EmployeeRequest request, Guid userId);
        Task DeleteEmployee(Guid employeeId, Guid userId);
        Task<BaseDatatableResponse> PayrollDatatable(BaseDatatableRequest request);
        Task CreatePayroll(CreatePayrollRequest request, Guid userId);
        Task<PayrollDetailViewModel?> GetPayrollById(Guid id);
        Task PostPayroll(Guid id, Guid userId);
        Task DeletePayroll(Guid id, Guid userId);
    }

    public class PayrollService : BaseService, IPayrollService
    {
        private static readonly Guid SalaryExpenseAccountId = Guid.Parse("50000000-0000-0000-0000-000000000003");
        private static readonly Guid PayrollPayableAccountId = Guid.Parse("20000000-0000-0000-0000-000000000020");

        private readonly IJournalEntryService _journalEntryService;

        public PayrollService(AppDbContext dbContext, IJournalEntryService journalEntryService) : base(dbContext)
        {
            _journalEntryService = journalEntryService;
        }

        public async Task<BaseDatatableResponse> EmployeeDatatable(BaseDatatableRequest request)
        {
            var query = _dbContext.Employees.Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.EmployeeCode.ToLower().Contains(search)
                    || x.FullName.ToLower().Contains(search)
                    || (x.Position != null && x.Position.ToLower().Contains(search))
                    || (x.Department != null && x.Department.ToLower().Contains(search)));
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderBy(x => x.EmployeeCode)
                .Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new EmployeeViewModel
                {
                    EmployeeId = x.EmployeeId,
                    EmployeeCode = x.EmployeeCode,
                    FullName = x.FullName,
                    Position = x.Position,
                    Department = x.Department,
                    HireDate = x.HireDate,
                    BasicSalary = x.BasicSalary,
                    BankAccountNumber = x.BankAccountNumber,
                    IsActive = x.IsActive
                }).ToListAsync();

            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task CreateEmployee(EmployeeRequest request, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(request.EmployeeCode)) throw new Exception("Employee code is required");
            if (string.IsNullOrWhiteSpace(request.FullName)) throw new Exception("Employee name is required");

            var exists = await _dbContext.Employees.AnyAsync(x => !x.IsDeleted && x.EmployeeCode == request.EmployeeCode.Trim());
            if (exists) throw new Exception("Employee code already exists");

            _dbContext.Employees.Add(new EmployeeEntity
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeCode = request.EmployeeCode.Trim(),
                FullName = request.FullName.Trim(),
                Position = request.Position,
                Department = request.Department,
                HireDate = request.HireDate,
                BasicSalary = request.BasicSalary,
                BankAccountNumber = request.BankAccountNumber,
                IsActive = true,
                CreatedBy = userId.ToString()
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateEmployee(Guid employeeId, EmployeeRequest request, Guid userId)
        {
            var employee = await _dbContext.Employees.FirstOrDefaultAsync(x => x.EmployeeId == employeeId && !x.IsDeleted);
            if (employee == null) throw new Exception("Employee not found");
            if (string.IsNullOrWhiteSpace(request.FullName)) throw new Exception("Employee name is required");

            var exists = await _dbContext.Employees.AnyAsync(x => !x.IsDeleted && x.EmployeeId != employeeId && x.EmployeeCode == request.EmployeeCode.Trim());
            if (exists) throw new Exception("Employee code already exists");

            employee.EmployeeCode = request.EmployeeCode.Trim();
            employee.FullName = request.FullName.Trim();
            employee.Position = request.Position;
            employee.Department = request.Department;
            employee.HireDate = request.HireDate;
            employee.BasicSalary = request.BasicSalary;
            employee.BankAccountNumber = request.BankAccountNumber;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = userId.ToString();

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteEmployee(Guid employeeId, Guid userId)
        {
            var employee = await _dbContext.Employees.FirstOrDefaultAsync(x => x.EmployeeId == employeeId && !x.IsDeleted);
            if (employee == null) throw new Exception("Employee not found");

            var usedInPayroll = await _dbContext.PayrollLines.AnyAsync(x => x.EmployeeId == employeeId);
            if (usedInPayroll) throw new Exception("Cannot delete employee that already has payroll records");

            employee.IsDeleted = true;
            employee.DeletedAt = DateTime.UtcNow;
            employee.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<BaseDatatableResponse> PayrollDatatable(BaseDatatableRequest request)
        {
            var query = _dbContext.Payrolls
                .Include(x => x.JournalEntry)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.PayrollNumber.ToLower().Contains(search)
                    || x.Notes != null && x.Notes.ToLower().Contains(search));
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(x => x.PeriodYear).ThenByDescending(x => x.PeriodMonth)
                .Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new PayrollViewModel
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
                    Notes = x.Notes,
                    LineCount = x.Lines.Count
                }).ToListAsync();

            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task CreatePayroll(CreatePayrollRequest request, Guid userId)
        {
            if (request.PeriodMonth < 1 || request.PeriodMonth > 12) throw new Exception("Invalid period month");
            if (request.PeriodYear < 2000) throw new Exception("Invalid period year");

            var exists = await _dbContext.Payrolls.AnyAsync(x => !x.IsDeleted && x.PeriodMonth == request.PeriodMonth && x.PeriodYear == request.PeriodYear);
            if (exists) throw new Exception($"Payroll for {request.PeriodMonth}/{request.PeriodYear} already exists");

            var employees = await _dbContext.Employees
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.EmployeeCode)
                .ToListAsync();
            if (employees.Count == 0) throw new Exception("No active employees found");

            var payroll = new PayrollEntity
            {
                PayrollId = Guid.NewGuid(),
                PayrollNumber = await GenerateNumberAsync(request.PeriodMonth, request.PeriodYear),
                PeriodMonth = request.PeriodMonth,
                PeriodYear = request.PeriodYear,
                PayrollDate = request.PayrollDate,
                Status = "Draft",
                Notes = request.Notes,
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

            _dbContext.Payrolls.Add(payroll);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<PayrollDetailViewModel?> GetPayrollById(Guid id)
        {
            return await _dbContext.Payrolls
                .Include(x => x.JournalEntry)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(l => l.Employee)
                .Where(x => x.PayrollId == id && !x.IsDeleted)
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
                }).FirstOrDefaultAsync();
        }

        public async Task PostPayroll(Guid id, Guid userId)
        {
            var payroll = await _dbContext.Payrolls
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.PayrollId == id && !x.IsDeleted);
            if (payroll == null) throw new Exception("Payroll not found");
            if (payroll.Status != "Draft") throw new Exception("Only draft payroll can be posted");
            if (payroll.Lines.Count == 0) throw new Exception("Payroll has no salary lines");

            var description = $"Auto journal for payroll {payroll.PayrollNumber}";
            var journalId = await _journalEntryService.CreateAsync(new CreateJournalEntryRequest
            {
                JournalDate = payroll.PayrollDate,
                Description = description,
                JournalLines = new List<JournalLineRequest>
                {
                    new() { AccountId = SalaryExpenseAccountId, Description = description, DebitAmount = payroll.TotalNetSalary, CreditAmount = 0 },
                    new() { AccountId = PayrollPayableAccountId, Description = description, DebitAmount = 0, CreditAmount = payroll.TotalNetSalary }
                }
            }, userId);

            await _journalEntryService.PostAsync(new PostJournalRequest { JournalId = journalId, PostedDate = payroll.PayrollDate }, userId);

            payroll.JournalId = journalId;
            payroll.Status = "Posted";
            payroll.UpdatedAt = DateTime.UtcNow;
            payroll.UpdatedBy = userId.ToString();

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeletePayroll(Guid id, Guid userId)
        {
            var payroll = await _dbContext.Payrolls.FirstOrDefaultAsync(x => x.PayrollId == id && !x.IsDeleted);
            if (payroll == null) throw new Exception("Payroll not found");
            if (payroll.Status != "Draft") throw new Exception("Only draft payroll can be deleted");

            payroll.IsDeleted = true;
            payroll.DeletedAt = DateTime.UtcNow;
            payroll.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        private async Task<string> GenerateNumberAsync(int month, int year)
        {
            var prefix = $"PR-{year}{month:D2}-";
            var last = await _dbContext.Payrolls.Where(x => x.PayrollNumber.StartsWith(prefix))
                .OrderByDescending(x => x.PayrollNumber).Select(x => x.PayrollNumber).FirstOrDefaultAsync();
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[prefix.Length..]) + 1;
            return $"{prefix}{next:D4}";
        }
    }
}

