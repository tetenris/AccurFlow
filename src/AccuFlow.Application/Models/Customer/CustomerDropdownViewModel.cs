namespace AccuFlow.Models.Customer
{
    public class CustomerDropdownViewModel
    {
        public Guid CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string DisplayText => $"{CustomerCode} - {CustomerName}";
    }
}
