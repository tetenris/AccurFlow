namespace AccuFlow.Models.AgingReport
{
    public class AgingReportRequest
    {
        public string AgingType { get; set; } = "AR";
        public DateTime AsOfDate { get; set; } = DateTime.Today;
    }

    public class AgingReportViewModel
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string PartnerName { get; set; } = string.Empty;
        public decimal Current { get; set; }
        public decimal Days1To30 { get; set; }
        public decimal Days31To60 { get; set; }
        public decimal Days61To90 { get; set; }
        public decimal Over90 { get; set; }
        public decimal Total => Current + Days1To30 + Days31To60 + Days61To90 + Over90;
    }
}
