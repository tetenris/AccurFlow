using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.Customer
{
    public class UpdateCustomerRequest : CreateCustomerRequest
    {
        [Required(ErrorMessage = "Customer ID is required")]
        public Guid CustomerId { get; set; }
    }
}
