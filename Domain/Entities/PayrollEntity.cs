using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class EmployeeEntity : BaseEntity
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string? Department { get; set; }
        public DateTime? HireDate { get; set; }
        public decimal BasicSalary { get; set; }
        public string? BankAccountNumber { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class PayrollEntity : BaseEntity
    {
        public Guid PayrollId { get; set; }
        public string PayrollNumber { get; set; } = string.Empty;
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public DateTime PayrollDate { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Posted
        public decimal TotalAllowances { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetSalary { get; set; }
        public Guid? JournalId { get; set; }
        public string? Notes { get; set; }
        public JournalEntryEntity? JournalEntry { get; set; }
        public virtual ICollection<PayrollLineEntity> Lines { get; set; } = new List<PayrollLineEntity>();
    }

    public class PayrollLineEntity : BaseEntity
    {
        public Guid PayrollLineId { get; set; }
        public Guid PayrollId { get; set; }
        public Guid EmployeeId { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public PayrollEntity Payroll { get; set; } = null!;
        public EmployeeEntity Employee { get; set; } = null!;
    }
}
