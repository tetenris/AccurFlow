using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class SupplierEntity : BaseEntity
    {
        public Guid SupplierId { get; set; }
        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierType { get; set; } = string.Empty; // Individual, Corporate, Government
        
        // Contact Information
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        
        // Address Information
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        
        // Financial Information
        public decimal CreditLimit { get; set; }
        public int PaymentTerms { get; set; } // in days
        public decimal CurrentBalance { get; set; }
        
        // Tax Information
        public string? TaxId { get; set; }
        
        // Additional Information
        public string? Notes { get; set; }
        
        // Status
        public bool IsActive { get; set; }
    }
}
