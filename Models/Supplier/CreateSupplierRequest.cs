using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.Supplier
{
    public class CreateSupplierRequest
    {
        [Required(ErrorMessage = "Supplier code is required")]
        [MaxLength(20, ErrorMessage = "Supplier code cannot exceed 20 characters")]
        public string SupplierCode { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Supplier name is required")]
        [MaxLength(255, ErrorMessage = "Supplier name cannot exceed 255 characters")]
        public string SupplierName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Supplier type is required")]
        public string SupplierType { get; set; } = string.Empty;
        
        [MaxLength(255, ErrorMessage = "Contact person cannot exceed 255 characters")]
        public string? ContactPerson { get; set; }
        
        [MaxLength(50, ErrorMessage = "Phone cannot exceed 50 characters")]
        [RegularExpression(@"^[\d\s\-\(\)]+$", ErrorMessage = "Phone number format is invalid")]
        public string? Phone { get; set; }
        
        [EmailAddress(ErrorMessage = "Email format is invalid")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string? Email { get; set; }
        
        [MaxLength(255, ErrorMessage = "Website cannot exceed 255 characters")]
        public string? Website { get; set; }
        
        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }
        
        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }
        
        [MaxLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        public string? State { get; set; }
        
        [MaxLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string? PostalCode { get; set; }
        
        [MaxLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string? Country { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Credit limit must be a non-negative value")]
        public decimal CreditLimit { get; set; } = 0;
        
        [Range(1, 365, ErrorMessage = "Payment terms must be between 1 and 365 days")]
        public int PaymentTerms { get; set; } = 30;
        
        [MaxLength(50, ErrorMessage = "Tax ID cannot exceed 50 characters")]
        public string? TaxId { get; set; }
        
        [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
