using AccuFlow.Infrastructure.Persistence;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.FixedAsset;
using AccuFlow.Models.JournalEntry;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface IFixedAssetService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableFixedAssetRequest request);
        Task<FixedAssetDetailViewModel?> GetById(Guid id);
        Task Create(CreateFixedAssetRequest request, Guid userId);
        Task Update(UpdateFixedAssetRequest request, Guid userId);
        Task Delete(Guid id, Guid userId);
        Task<FixedAssetDepreciationViewModel> Depreciate(Guid id, DateTime periodDate, Guid userId);
    }

    public class FixedAssetService : BaseService, IFixedAssetService
    {
        private static readonly Guid DefaultAssetAccountId = Guid.Parse("10000000-0000-0000-0000-000000000006");
        private static readonly Guid DefaultAccumulatedDepreciationAccountId = Guid.Parse("10000000-0000-0000-0000-000000000007");
        private static readonly Guid DefaultDepreciationExpenseAccountId = Guid.Parse("50000000-0000-0000-0000-000000000006");

        private readonly IJournalEntryService _journalEntryService;

        public FixedAssetService(AppDbContext dbContext, IJournalEntryService journalEntryService) : base(dbContext)
        {
            _journalEntryService = journalEntryService;
        }

        public async Task<BaseDatatableResponse> Datatable(DataTableFixedAssetRequest request)
        {
            var query = _dbContext.FixedAssets.Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.Category)) query = query.Where(x => x.Category == request.Category);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.AssetCode.ToLower().Contains(search) || x.AssetName.ToLower().Contains(search));
            }
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.PurchaseDate).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new FixedAssetViewModel
                {
                    AssetId = x.AssetId,
                    AssetCode = x.AssetCode,
                    AssetName = x.AssetName,
                    Category = x.Category,
                    PurchaseDate = x.PurchaseDate,
                    PurchaseCost = x.PurchaseCost,
                    SalvageValue = x.SalvageValue,
                    UsefulLifeMonths = x.UsefulLifeMonths,
                    AccumulatedDepreciation = x.AccumulatedDepreciation,
                    Status = x.Status
                }).ToListAsync();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<FixedAssetDetailViewModel?> GetById(Guid id)
        {
            return await _dbContext.FixedAssets
                .Include(x => x.Depreciations.Where(d => !d.IsDeleted))
                .Where(x => x.AssetId == id && !x.IsDeleted)
                .Select(x => new FixedAssetDetailViewModel
                {
                    AssetId = x.AssetId,
                    AssetCode = x.AssetCode,
                    AssetName = x.AssetName,
                    Category = x.Category,
                    PurchaseDate = x.PurchaseDate,
                    PurchaseCost = x.PurchaseCost,
                    SalvageValue = x.SalvageValue,
                    UsefulLifeMonths = x.UsefulLifeMonths,
                    AccumulatedDepreciation = x.AccumulatedDepreciation,
                    Status = x.Status,
                    AssetAccountId = x.AssetAccountId,
                    AccumulatedDepreciationAccountId = x.AccumulatedDepreciationAccountId,
                    DepreciationExpenseAccountId = x.DepreciationExpenseAccountId,
                    LastDepreciationDate = x.LastDepreciationDate,
                    Notes = x.Notes,
                    Depreciations = x.Depreciations.Select(d => new FixedAssetDepreciationViewModel
                    {
                        DepreciationId = d.DepreciationId,
                        PeriodDate = d.PeriodDate,
                        Amount = d.Amount,
                        JournalId = d.JournalId
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task Create(CreateFixedAssetRequest request, Guid userId)
        {
            if (request.PurchaseCost <= 0) throw new Exception("Purchase cost must be greater than zero");
            if (request.SalvageValue >= request.PurchaseCost) throw new Exception("Salvage value must be less than purchase cost");
            if (request.UsefulLifeMonths <= 0) throw new Exception("Useful life must be greater than zero");

            _dbContext.FixedAssets.Add(new FixedAssetEntity
            {
                AssetId = Guid.NewGuid(),
                AssetCode = await GenerateNumberAsync(),
                AssetName = request.AssetName,
                Category = request.Category,
                PurchaseDate = request.PurchaseDate,
                PurchaseCost = request.PurchaseCost,
                SalvageValue = request.SalvageValue,
                UsefulLifeMonths = request.UsefulLifeMonths,
                AssetAccountId = request.AssetAccountId ?? DefaultAssetAccountId,
                AccumulatedDepreciationAccountId = request.AccumulatedDepreciationAccountId ?? DefaultAccumulatedDepreciationAccountId,
                DepreciationExpenseAccountId = request.DepreciationExpenseAccountId ?? DefaultDepreciationExpenseAccountId,
                Status = "Active",
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(UpdateFixedAssetRequest request, Guid userId)
        {
            var asset = await _dbContext.FixedAssets.FirstOrDefaultAsync(x => x.AssetId == request.AssetId && !x.IsDeleted);
            if (asset == null) throw new Exception("Fixed asset not found");
            if (request.PurchaseCost <= 0) throw new Exception("Purchase cost must be greater than zero");
            if (request.SalvageValue >= request.PurchaseCost) throw new Exception("Salvage value must be less than purchase cost");
            if (request.UsefulLifeMonths <= 0) throw new Exception("Useful life must be greater than zero");

            asset.AssetName = request.AssetName;
            asset.Category = request.Category;
            asset.PurchaseDate = request.PurchaseDate;
            asset.PurchaseCost = request.PurchaseCost;
            asset.SalvageValue = request.SalvageValue;
            asset.UsefulLifeMonths = request.UsefulLifeMonths;
            asset.AssetAccountId = request.AssetAccountId ?? asset.AssetAccountId;
            asset.AccumulatedDepreciationAccountId = request.AccumulatedDepreciationAccountId ?? asset.AccumulatedDepreciationAccountId;
            asset.DepreciationExpenseAccountId = request.DepreciationExpenseAccountId ?? asset.DepreciationExpenseAccountId;
            asset.Notes = request.Notes;
            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var asset = await _dbContext.FixedAssets.FirstOrDefaultAsync(x => x.AssetId == id && !x.IsDeleted);
            if (asset == null) throw new Exception("Fixed asset not found");
            if (await _dbContext.FixedAssetDepreciations.AnyAsync(x => x.AssetId == id && !x.IsDeleted)) throw new Exception("Asset with depreciation history cannot be deleted");
            asset.IsDeleted = true;
            asset.DeletedAt = DateTime.UtcNow;
            asset.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<FixedAssetDepreciationViewModel> Depreciate(Guid id, DateTime periodDate, Guid userId)
        {
            var asset = await _dbContext.FixedAssets.FirstOrDefaultAsync(x => x.AssetId == id && !x.IsDeleted);
            if (asset == null) throw new Exception("Fixed asset not found");
            if (asset.Status != "Active") throw new Exception("Only active asset can be depreciated");

            var monthly = (asset.PurchaseCost - asset.SalvageValue) / asset.UsefulLifeMonths;
            var lastPeriod = asset.LastDepreciationDate ?? asset.PurchaseDate.AddMonths(-1);
            var monthsToRun = ((periodDate.Year - lastPeriod.Year) * 12) + (periodDate.Month - lastPeriod.Month);
            if (monthsToRun <= 0) throw new Exception("No depreciation due for this period");
            var capMonths = asset.UsefulLifeMonths;
            var runMonths = Math.Min(monthsToRun, capMonths);
            if (runMonths <= 0) throw new Exception("Asset is fully depreciated");

            var amount = decimal.Round(monthly * runMonths, 2);
            var usable = asset.PurchaseCost - asset.SalvageValue - asset.AccumulatedDepreciation;
            if (amount > usable) amount = usable;
            if (amount <= 0) throw new Exception("Asset is fully depreciated");

            var journalLines = new List<JournalLineRequest>
            {
                new JournalLineRequest
                {
                    AccountId = asset.DepreciationExpenseAccountId ?? DefaultDepreciationExpenseAccountId,
                    Description = $"Depreciation for {asset.AssetCode} - {asset.AssetName}",
                    DebitAmount = amount,
                    CreditAmount = 0
                },
                new JournalLineRequest
                {
                    AccountId = asset.AccumulatedDepreciationAccountId ?? DefaultAccumulatedDepreciationAccountId,
                    Description = $"Depreciation for {asset.AssetCode} - {asset.AssetName}",
                    DebitAmount = 0,
                    CreditAmount = amount
                }
            };
            var journalId = await _journalEntryService.CreateAsync(new CreateJournalEntryRequest
            {
                JournalDate = periodDate,
                Description = $"Depreciation for {asset.AssetCode} - {asset.AssetName}",
                JournalLines = journalLines
            }, userId);
            await _journalEntryService.PostAsync(new PostJournalRequest { JournalId = journalId, PostedDate = periodDate }, userId);

            asset.AccumulatedDepreciation += amount;
            asset.LastDepreciationDate = periodDate;
            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = userId.ToString();
            var deprec = new FixedAssetDepreciationEntity
            {
                DepreciationId = Guid.NewGuid(),
                AssetId = asset.AssetId,
                PeriodDate = periodDate,
                Amount = amount,
                JournalId = journalId,
                CreatedBy = userId.ToString()
            };
            _dbContext.FixedAssetDepreciations.Add(deprec);
            await _dbContext.SaveChangesAsync();

            return new FixedAssetDepreciationViewModel
            {
                DepreciationId = deprec.DepreciationId,
                PeriodDate = deprec.PeriodDate,
                Amount = deprec.Amount,
                JournalId = deprec.JournalId
            };
        }

        private async Task<string> GenerateNumberAsync()
        {
            var last = await _dbContext.FixedAssets.Where(x => x.AssetCode.StartsWith("FA-"))
                .OrderByDescending(x => x.AssetCode).Select(x => x.AssetCode).FirstOrDefaultAsync();
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[3..]) + 1;
            return $"FA-{next:D5}";
        }
    }
}

