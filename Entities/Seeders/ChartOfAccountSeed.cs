using AccuFlow.Entities.Entity;

namespace AccuFlow.Entities.Seeders
{
    public static class ChartOfAccountSeed
    {
        public static List<ChartOfAccountEntity> GetChartOfAccountSeedData()
        {
            return new List<ChartOfAccountEntity>
            {
                // ASSETS
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                    AccountCode = "1-10000",
                    AccountName = "Current Assets",
                    AccountType = "Asset",
                    Description = "Assets that can be converted to cash within one year",
                    IsHeader = true,
                    IsActive = true,
                    NormalBalance = "Debit",
                    Level = 0,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                    AccountCode = "1-10100",
                    AccountName = "Cash",
                    AccountType = "Asset",
                    Description = "Cash on hand",
                    ParentAccountId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Debit",
                    Level = 1,
                    AccountUsage = 1, // Cash
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                    AccountCode = "1-10200",
                    AccountName = "Cash in Bank",
                    AccountType = "Asset",
                    Description = "Cash deposited in bank accounts",
                    ParentAccountId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Debit",
                    Level = 1,
                    AccountUsage = 2, // Bank
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                    AccountCode = "1-10300",
                    AccountName = "Accounts Receivable",
                    AccountType = "Asset",
                    Description = "Money owed by customers",
                    ParentAccountId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Debit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                    AccountCode = "1-10400",
                    AccountName = "Inventory",
                    AccountType = "Asset",
                    Description = "Goods available for sale",
                    ParentAccountId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Debit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },

                // LIABILITIES
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                    AccountCode = "2-10000",
                    AccountName = "Current Liabilities",
                    AccountType = "Liability",
                    Description = "Obligations due within one year",
                    IsHeader = true,
                    IsActive = true,
                    NormalBalance = "Credit",
                    Level = 0,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                    AccountCode = "2-10100",
                    AccountName = "Accounts Payable",
                    AccountType = "Liability",
                    Description = "Money owed to suppliers",
                    ParentAccountId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Credit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("20000000-0000-0000-0000-000000000003"),
                    AccountCode = "2-10200",
                    AccountName = "Tax Payable",
                    AccountType = "Liability",
                    Description = "Taxes owed to government",
                    ParentAccountId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Credit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("20000000-0000-0000-0000-000000000020"),
                    AccountCode = "2-10300",
                    AccountName = "Payroll Payable",
                    AccountType = "Liability",
                    Description = "Accrued employee salaries payable",
                    ParentAccountId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Credit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },

                // EQUITY
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                    AccountCode = "3-10000",
                    AccountName = "Owner's Equity",
                    AccountType = "Equity",
                    Description = "Owner's investment and retained earnings",
                    IsHeader = true,
                    IsActive = true,
                    NormalBalance = "Credit",
                    Level = 0,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("30000000-0000-0000-0000-000000000002"),
                    AccountCode = "3-10100",
                    AccountName = "Capital",
                    AccountType = "Equity",
                    Description = "Owner's capital investment",
                    ParentAccountId = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Credit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                    AccountCode = "3-10200",
                    AccountName = "Retained Earnings",
                    AccountType = "Equity",
                    Description = "Accumulated profits",
                    ParentAccountId = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Credit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },

                // REVENUE
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                    AccountCode = "4-10000",
                    AccountName = "Revenue",
                    AccountType = "Revenue",
                    Description = "Income from business operations",
                    IsHeader = true,
                    IsActive = true,
                    NormalBalance = "Credit",
                    Level = 0,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("40000000-0000-0000-0000-000000000002"),
                    AccountCode = "4-10100",
                    AccountName = "Sales Revenue",
                    AccountType = "Revenue",
                    Description = "Revenue from sales of goods or services",
                    ParentAccountId = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Credit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("40000000-0000-0000-0000-000000000003"),
                    AccountCode = "4-10200",
                    AccountName = "Service Revenue",
                    AccountType = "Revenue",
                    Description = "Revenue from services provided",
                    ParentAccountId = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Credit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },

                // EXPENSES
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                    AccountCode = "5-10000",
                    AccountName = "Operating Expenses",
                    AccountType = "Expense",
                    Description = "Expenses from business operations",
                    IsHeader = true,
                    IsActive = true,
                    NormalBalance = "Debit",
                    Level = 0,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("50000000-0000-0000-0000-000000000002"),
                    AccountCode = "5-10100",
                    AccountName = "Cost of Goods Sold",
                    AccountType = "Expense",
                    Description = "Direct costs of producing goods sold",
                    ParentAccountId = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Debit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("50000000-0000-0000-0000-000000000003"),
                    AccountCode = "5-10200",
                    AccountName = "Salary Expense",
                    AccountType = "Expense",
                    Description = "Employee salaries and wages",
                    ParentAccountId = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Debit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("50000000-0000-0000-0000-000000000004"),
                    AccountCode = "5-10300",
                    AccountName = "Rent Expense",
                    AccountType = "Expense",
                    Description = "Office or facility rent",
                    ParentAccountId = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Debit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("50000000-0000-0000-0000-000000000005"),
                    AccountCode = "5-10400",
                    AccountName = "Utilities Expense",
                    AccountType = "Expense",
                    Description = "Electricity, water, internet, etc.",
                    ParentAccountId = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Debit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },

                // OTHER INCOME
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("60000000-0000-0000-0000-000000000001"),
                    AccountCode = "6-10000",
                    AccountName = "Other Income",
                    AccountType = "Other Income",
                    Description = "Non-operating income and gains",
                    IsHeader = true,
                    IsActive = true,
                    NormalBalance = "Credit",
                    Level = 0,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("60000000-0000-0000-0000-000000000002"),
                    AccountCode = "6-10100",
                    AccountName = "Interest Income",
                    AccountType = "Other Income",
                    Description = "Interest earned from bank deposits",
                    ParentAccountId = Guid.Parse("60000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Credit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("60000000-0000-0000-0000-000000000003"),
                    AccountCode = "6-10200",
                    AccountName = "Gain on Asset Sale",
                    AccountType = "Other Income",
                    Description = "Profit from selling fixed assets",
                    ParentAccountId = Guid.Parse("60000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Credit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("60000000-0000-0000-0000-000000000004"),
                    AccountCode = "6-10300",
                    AccountName = "Foreign Exchange Gain",
                    AccountType = "Other Income",
                    Description = "Gain from currency exchange differences",
                    ParentAccountId = Guid.Parse("60000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Credit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },

                // OTHER EXPENSE
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("70000000-0000-0000-0000-000000000001"),
                    AccountCode = "7-10000",
                    AccountName = "Other Expense",
                    AccountType = "Other Expense",
                    Description = "Non-operating expenses and losses",
                    IsHeader = true,
                    IsActive = true,
                    NormalBalance = "Debit",
                    Level = 0,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("70000000-0000-0000-0000-000000000002"),
                    AccountCode = "7-10100",
                    AccountName = "Interest Expense",
                    AccountType = "Other Expense",
                    Description = "Interest paid on loans and debts",
                    ParentAccountId = Guid.Parse("70000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Debit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("70000000-0000-0000-0000-000000000003"),
                    AccountCode = "7-10200",
                    AccountName = "Loss on Asset Sale",
                    AccountType = "Other Expense",
                    Description = "Loss from selling fixed assets",
                    ParentAccountId = Guid.Parse("70000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Debit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("70000000-0000-0000-0000-000000000004"),
                    AccountCode = "7-10300",
                    AccountName = "Foreign Exchange Loss",
                    AccountType = "Other Expense",
                    Description = "Loss from currency exchange differences",
                    ParentAccountId = Guid.Parse("70000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Debit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("10000000-0000-0000-0000-000000000006"),
                    AccountCode = "1-10500",
                    AccountName = "Fixed Assets",
                    AccountType = "Asset",
                    Description = "Property, plant and equipment owned by the company",
                    ParentAccountId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Debit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("10000000-0000-0000-0000-000000000007"),
                    AccountCode = "1-10600",
                    AccountName = "Accumulated Depreciation",
                    AccountType = "Asset",
                    Description = "Accumulated depreciation on fixed assets (contra asset)",
                    ParentAccountId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Credit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccountEntity
                {
                    AccountId = Guid.Parse("50000000-0000-0000-0000-000000000006"),
                    AccountCode = "5-10500",
                    AccountName = "Depreciation Expense",
                    AccountType = "Expense",
                    Description = "Depreciation charged on fixed assets",
                    ParentAccountId = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                    IsHeader = false,
                    IsActive = true,
                    NormalBalance = "Debit",
                    Level = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };
        }
    }
}
