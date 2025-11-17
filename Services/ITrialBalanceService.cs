using AccuFlow.Services;
using KomatsuERP.Models.TrialBalance;

namespace KomatsuERP.Services;

public interface ITrialBalanceService : IBaseService
{
    Task<TrialBalanceViewModel> GetTrialBalanceAsync(GetTrialBalanceRequest request);
    Task<byte[]> ExportToExcelAsync(GetTrialBalanceRequest request);
}
