using AccuFlow.Services;
using AccuFlow.Models.TrialBalance;

namespace AccuFlow.Services;

public interface ITrialBalanceService : IBaseService
{
    Task<TrialBalanceViewModel> GetTrialBalanceAsync(GetTrialBalanceRequest request);
    Task<byte[]> ExportToExcelAsync(GetTrialBalanceRequest request);
}
