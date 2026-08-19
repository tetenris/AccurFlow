using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.ChartOfAccount
{
    public class UpdateChartOfAccountRequest
    {
        [Required(ErrorMessage = "Account ID is required")]
        public Guid AccountId { get; set; }

        [Required(ErrorMessage = "Account code is required")]
        [StringLength(20, ErrorMessage = "Account code cannot exceed 20 characters")]
        public string AccountCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account name is required")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Account name must be between 3 and 255 characters")]
        public string AccountName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account type is required")]
        public string AccountType { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        public Guid? ParentAccountId { get; set; }

        public bool IsHeader { get; set; }

        public bool IsActive { get; set; }

        public decimal OpeningBalance { get; set; }

        [StringLength(10, ErrorMessage = "Currency code cannot exceed 10 characters")]
        public string Currency { get; set; } = string.Empty;
    }
}
