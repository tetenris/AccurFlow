using AccuFlow.Services;
using AccuFlow.Models.FinancialStatement;

namespace AccuFlow.Services;

public interface IFinancialStatementService : IBaseService
{
    Task<IncomeStatementViewModel> GetIncomeStatementAsync(GetIncomeStatementRequest request);
    Task<BalanceSheetViewModel> GetBalanceSheetAsync(GetBalanceSheetRequest request);
    Task<CashFlowStatementViewModel> GetCashFlowStatementAsync(GetCashFlowStatementRequest request);
    Task<byte[]> ExportIncomeStatementAsync(GetIncomeStatementRequest request);
    Task<byte[]> ExportBalanceSheetAsync(GetBalanceSheetRequest request);
    Task<byte[]> ExportCashFlowAsync(GetCashFlowStatementRequest request);
}
