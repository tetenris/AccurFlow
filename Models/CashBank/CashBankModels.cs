using AccuFlow.Models.BaseModel;
using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.CashBank
{
    public class BankAccountViewModel
    {
        public Guid AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public int Usage { get; set; }
        public decimal Balance { get; set; }
    }

    public class DataTableTransferRequest : BaseDatatableRequest
    {
        public string? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class TransferViewModel
    {
        public Guid TransferId { get; set; }
        public string TransferNumber { get; set; } = string.Empty;
        public DateTime TransferDate { get; set; }
        public string FromAccountName { get; set; } = string.Empty;
        public string ToAccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CreateTransferRequest
    {
        public DateTime TransferDate { get; set; } = DateTime.Today;
        [Required]
        public Guid FromAccountId { get; set; }
        [Required]
        public Guid ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
    }

    public class UpdateTransferRequest : CreateTransferRequest
    {
        public Guid TransferId { get; set; }
    }

    public class TransferDetailViewModel : TransferViewModel
    {
        public Guid FromAccountId { get; set; }
        public Guid ToAccountId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public string? JournalNumber { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPost { get; set; }
    }

    public class DataTableReconciliationRequest : BaseDatatableRequest
    {
        public string? Status { get; set; }
    }

    public class ReconciliationViewModel
    {
        public Guid ReconciliationId { get; set; }
        public string ReconciliationNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public DateTime StatementDate { get; set; }
        public decimal StatementEndingBalance { get; set; }
        public decimal GlEndingBalance { get; set; }
        public string Status { get; set; } = string.Empty;
        public int LineCount { get; set; }
        public int ClearedCount { get; set; }
        public decimal Difference => StatementEndingBalance - GlEndingBalance;
    }

    public class ReconciliationLineViewModel
    {
        public Guid ReconciliationLineId { get; set; }
        public Guid ReconciliationId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsCleared { get; set; }
    }

    public class ReconciliationLineRequest
    {
        public Guid? ReconciliationLineId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsCleared { get; set; }
    }

    public class CreateReconciliationRequest
    {
        [Required]
        public Guid AccountId { get; set; }
        public DateTime StatementDate { get; set; } = DateTime.Today;
        public decimal StatementEndingBalance { get; set; }
        public decimal GlEndingBalance { get; set; }
        public string? Notes { get; set; }
        public List<ReconciliationLineRequest> Lines { get; set; } = new();
    }

    public class UpdateReconciliationRequest : CreateReconciliationRequest
    {
        public Guid ReconciliationId { get; set; }
    }

    public class ReconciliationDetailViewModel : ReconciliationViewModel
    {
        public Guid AccountId { get; set; }
        public string? Notes { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPost { get; set; }
        public List<ReconciliationLineViewModel> Lines { get; set; } = new();
    }
}