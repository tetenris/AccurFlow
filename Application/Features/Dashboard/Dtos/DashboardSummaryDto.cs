namespace AccuFlow.Application.Features.Dashboard.Dtos
{
    public class DashboardSummaryDto
    {
        public int ActiveUsers { get; set; }
        public int ActiveRoles { get; set; }
        public int DraftJournals { get; set; }
        public int PostedJournals { get; set; }
        public int OpenInvoices { get; set; }
        public int OverdueInvoices { get; set; }
        public int PendingPurchaseOrders { get; set; }
        public decimal Receivables { get; set; }
        public decimal Payables { get; set; }
        public decimal CashMovementsThisMonth { get; set; }
        public List<DashboardActivityDto> RecentActivities { get; set; } = new();
    }

    public class DashboardActivityDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}