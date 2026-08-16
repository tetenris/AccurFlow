using AccuFlow.Models.BaseModel;
using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.Tax
{
    public class DataTableTaxRequest : BaseDatatableRequest { }

    public class TaxViewModel
    {
        public Guid TaxId { get; set; }
        public string TaxCode { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string TaxType { get; set; } = "VAT";
        public bool IsActive { get; set; }
    }

    public class CreateTaxRequest
    {
        [Required]
        public string TaxCode { get; set; } = string.Empty;
        [Required]
        public string TaxName { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string TaxType { get; set; } = "VAT";
    }

    public class UpdateTaxRequest : CreateTaxRequest
    {
        public Guid TaxId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class VatReportRequest
    {
        public DateTime FromDate { get; set; } = new DateTime(DateTime.Today.Year, 1, 1);
        public DateTime ToDate { get; set; } = DateTime.Today;
    }

    public class VatReportSummary
    {
        public decimal OutputVat { get; set; }
        public decimal InputVat { get; set; }
        public decimal NetVat => OutputVat - InputVat;
    }

    public class VatReportLine
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string InvoiceType { get; set; } = string.Empty;
        public string PartnerName { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public decimal Dpp { get; set; }
        public decimal Vat { get; set; }
        public decimal TotalAmount { get; set; }
    }
}