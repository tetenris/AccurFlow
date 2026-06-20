namespace AccuFlow.Models.Supplier
{
    public class SupplierDropdownViewModel
    {
        public Guid SupplierId { get; set; }
        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string DisplayText => $"{SupplierCode} - {SupplierName}";
    }
}
