namespace AccuFlow.Models.ReceivablePayable
{
    public class ReceivablePayableRequest
    {
        public string ReportType { get; set; } = "AR";
        public DateTime AsOfDate { get; set; } = DateTime.Today;
    }

    public class ReceivablePayableViewModel
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string PartnerName { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}