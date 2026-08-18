namespace AccuFlow.Application.Common.Helpers
{
    public static class GeneralLedgerHelper
    {
        public static decimal CalculateBalance(string accountType, decimal totalDebit, decimal totalCredit)
        {
            return accountType switch
            {
                "Asset" => totalDebit - totalCredit,
                "Expense" => totalDebit - totalCredit,
                "Other Expense" => totalDebit - totalCredit,
                "Liability" => totalCredit - totalDebit,
                "Equity" => totalCredit - totalDebit,
                "Revenue" => totalCredit - totalDebit,
                "Other Income" => totalCredit - totalDebit,
                _ => 0
            };
        }

        public static bool IsDebitBalance(string accountType)
        {
            return accountType is "Asset" or "Expense" or "Other Expense";
        }
    }
}