using AccuFlow.Models.BaseModel;
using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.FixedAsset
{
    public class DataTableFixedAssetRequest : BaseDatatableRequest
    {
        public string? Category { get; set; }
        public string? Status { get; set; }
    }

    public class FixedAssetViewModel
    {
        public Guid AssetId { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public decimal PurchaseCost { get; set; }
        public decimal SalvageValue { get; set; }
        public int UsefulLifeMonths { get; set; }
        public decimal AccumulatedDepreciation { get; set; }
        public decimal MonthlyDepreciation => UsefulLifeMonths > 0 ? (PurchaseCost - SalvageValue) / UsefulLifeMonths : 0;
        public decimal NetBookValue => PurchaseCost - AccumulatedDepreciation;
        public string Status { get; set; } = "Active";
    }

    public class FixedAssetDetailViewModel : FixedAssetViewModel
    {
        public Guid? AssetAccountId { get; set; }
        public Guid? AccumulatedDepreciationAccountId { get; set; }
        public Guid? DepreciationExpenseAccountId { get; set; }
        public DateTime? LastDepreciationDate { get; set; }
        public string? Notes { get; set; }
        public List<FixedAssetDepreciationViewModel> Depreciations { get; set; } = new();
    }

    public class CreateFixedAssetRequest
    {
        [Required]
        public string AssetName { get; set; } = string.Empty;
        public string Category { get; set; } = "Equipment";
        public DateTime PurchaseDate { get; set; } = DateTime.Today;
        public decimal PurchaseCost { get; set; }
        public decimal SalvageValue { get; set; }
        public int UsefulLifeMonths { get; set; } = 60;
        public Guid? AssetAccountId { get; set; }
        public Guid? AccumulatedDepreciationAccountId { get; set; }
        public Guid? DepreciationExpenseAccountId { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateFixedAssetRequest : CreateFixedAssetRequest
    {
        public Guid AssetId { get; set; }
    }

    public class FixedAssetDepreciationViewModel
    {
        public Guid DepreciationId { get; set; }
        public DateTime PeriodDate { get; set; }
        public decimal Amount { get; set; }
        public Guid? JournalId { get; set; }
    }

    public class DepreciateFixedAssetRequest
    {
        public Guid AssetId { get; set; }
        public DateTime PeriodDate { get; set; } = DateTime.Today;
    }
}