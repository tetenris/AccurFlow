using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.Supplier
{
    public class UpdateSupplierRequest : CreateSupplierRequest
    {
        [Required(ErrorMessage = "Supplier ID is required")]
        public Guid SupplierId { get; set; }
    }
}
