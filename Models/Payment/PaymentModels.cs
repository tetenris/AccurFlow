using AccuFlow.Models.BaseModel;
using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.Payment
{
    public class DataTablePaymentRequest : BaseDatatableRequest
    {
        public string? PaymentType { get; set; }
        public string? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class PaymentViewModel
    {
        public Guid PaymentId { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    public class CreatePaymentRequest
    {
        [Required]
        public string PaymentType { get; set; } = "Receipt";
        public DateTime PaymentDate { get; set; } = DateTime.Today;
        public Guid? CustomerId { get; set; }
        public Guid? SupplierId { get; set; }
        public Guid CashBankAccountId { get; set; }
        public string PaymentMethod { get; set; } = "BankTransfer";
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public List<CreatePaymentAllocationRequest> Allocations { get; set; } = new();
    }

    public class UpdatePaymentRequest : CreatePaymentRequest
    {
        public Guid PaymentId { get; set; }
    }

    public class PaymentDetailViewModel : PaymentViewModel
    {
        public Guid? CustomerId { get; set; }
        public Guid? SupplierId { get; set; }
        public Guid CashBankAccountId { get; set; }
        public string CashBankAccountName { get; set; } = string.Empty;
        public Guid? JournalId { get; set; }
        public string? JournalNumber { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPost { get; set; }
        public List<PaymentAllocationViewModel> Allocations { get; set; } = new();
    }

    public class PaymentAllocationViewModel
    {
        public Guid PaymentAllocationId { get; set; }
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal AllocatedAmount { get; set; }
    }

    public class CreatePaymentAllocationRequest
    {
        public Guid InvoiceId { get; set; }
        public decimal AllocatedAmount { get; set; }
    }
}
