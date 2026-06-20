using AccuFlow.Services;
using AccuFlow.Models.GeneralLedger;

namespace AccuFlow.Services;

public interface IGeneralLedgerService : IBaseService
{
    Task<AccountLedgerViewModel> GetAccountLedgerAsync(GetLedgerRequest request);
    Task<List<LedgerSummaryViewModel>> GetLedgerSummaryAsync(GetLedgerSummaryRequest request);
    Task<decimal> GetAccountBalanceAsync(Guid accountId, DateTime? asOfDate = null);
    Task<decimal> GetOpeningBalanceAsync(Guid accountId, DateTime dateFrom);
    Task<byte[]> ExportLedgerToExcelAsync(GetLedgerRequest request);
    Task<byte[]> ExportSummaryToExcelAsync(GetLedgerSummaryRequest request);
}
