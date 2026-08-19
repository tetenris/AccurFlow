namespace AccuFlow.Models.Payroll
{
    public class EmployeeRequest
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string? Department { get; set; }
        public DateTime? HireDate { get; set; }
        public decimal BasicSalary { get; set; }
        public string? BankAccountNumber { get; set; }
    }

    public class EmployeeViewModel
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string? Department { get; set; }
        public DateTime? HireDate { get; set; }
        public decimal BasicSalary { get; set; }
        public string? BankAccountNumber { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreatePayrollRequest
    {
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public DateTime PayrollDate { get; set; }
        public string? Notes { get; set; }
    }

    public class PayrollViewModel
    {
        public Guid PayrollId { get; set; }
        public string PayrollNumber { get; set; } = string.Empty;
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public DateTime PayrollDate { get; set; }
        public string Status { get; set; } = "Draft";
        public decimal TotalAllowances { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetSalary { get; set; }
        public string? JournalNumber { get; set; }
        public string? Notes { get; set; }
        public int LineCount { get; set; }
    }

    public class PayrollDetailViewModel
    {
        public Guid PayrollId { get; set; }
        public string PayrollNumber { get; set; } = string.Empty;
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public DateTime PayrollDate { get; set; }
        public string Status { get; set; } = "Draft";
        public decimal TotalAllowances { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetSalary { get; set; }
        public string? JournalNumber { get; set; }
        public bool CanPost => Status == "Draft";
        public List<PayrollLineViewModel> Lines { get; set; } = new();
    }

    public class PayrollLineViewModel
    {
        public Guid PayrollLineId { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
    }
}