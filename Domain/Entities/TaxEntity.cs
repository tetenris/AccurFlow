using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class TaxEntity : BaseEntity
    {
        public Guid TaxId { get; set; }
        public string TaxCode { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string TaxType { get; set; } = "VAT";
        public Guid? AccountId { get; set; }
        public bool IsActive { get; set; } = true;
        public ChartOfAccountEntity? Account { get; set; }
    }
}

