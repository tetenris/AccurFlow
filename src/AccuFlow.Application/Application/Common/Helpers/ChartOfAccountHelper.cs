namespace AccuFlow.Application.Common.Helpers
{
    public static class ChartOfAccountHelper
    {
        public static string GetNormalBalance(string accountType)
        {
            return accountType switch
            {
                "Asset" => "Debit",
                "Expense" => "Debit",
                "Other Expense" => "Debit",
                "Liability" => "Credit",
                "Equity" => "Credit",
                "Revenue" => "Credit",
                "Other Income" => "Credit",
                _ => "Debit"
            };
        }

        public static string GetAccountTypePrefix(string accountType)
        {
            return accountType switch
            {
                "Asset" => "1",
                "Liability" => "2",
                "Equity" => "3",
                "Revenue" => "4",
                "Expense" => "5",
                "Other Income" => "6",
                "Other Expense" => "7",
                _ => "1"
            };
        }

        public static bool TryParseYesNo(string value, out bool result)
        {
            switch (value.Trim().ToLowerInvariant())
            {
                case "yes":
                case "y":
                case "1":
                case "true":
                    result = true;
                    return true;
                case "no":
                case "n":
                case "0":
                case "false":
                    result = false;
                    return true;
                default:
                    result = false;
                    return false;
            }
        }
    }
}