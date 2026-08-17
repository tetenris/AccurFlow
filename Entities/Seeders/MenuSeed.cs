using AccuFlow.Entities.Entity;

namespace AccuFlow.Entities.Seeders
{
    public static class MenuSeed
    {
        public static List<MenuEntity> GetMenuSeedData()
        {
            return new List<MenuEntity>
            {
                // Dashboard
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    MenuParentId = null,
                    Icon = @"<i class=""ki-duotone ki-element-11 fs-2""><span class=""path1""></span><span class=""path2""></span><span class=""path3""></span><span class=""path4""></span></i>",
                    Name = "Dashboard",
                    Controller = "Home",
                    Action = @"[""view""]",
                    Sequence = 1
                },
                
                // Database Seeding
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    MenuParentId = null,
                    Icon = @"<i class=""ki-duotone ki-abstract-26 fs-2""><span class=""path1""></span><span class=""path2""></span></i>",
                    Name = "Database Seeding",
                    Controller = "Seed",
                    Action = @"[""view""]",
                    Sequence = 2
                },
                
                // User Management (Parent)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    MenuParentId = null,
                    Icon = @"<i class=""ki-duotone ki-profile-user fs-2""><span class=""path1""></span><span class=""path2""></span><span class=""path3""></span><span class=""path4""></span></i>",
                    Name = "User Management",
                    Controller = "",
                    Action = @"[]",
                    Sequence = 3
                },
                
                // Users (Child of User Management)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    Icon = "",
                    Name = "Users",
                    Controller = "User",
                    Action = @"[""view"",""add"",""edit"",""delete""]",
                    Sequence = 1
                },
                
                // Roles (Child of User Management)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    Icon = "",
                    Name = "Roles",
                    Controller = "Role",
                    Action = @"[""view"",""add"",""edit"",""delete""]",
                    Sequence = 2
                },
                
                // Master (Parent)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000006"),
                    MenuParentId = null,
                    Icon = @"<i class=""ki-duotone ki-category fs-2""><span class=""path1""></span><span class=""path2""></span><span class=""path3""></span><span class=""path4""></span></i>",
                    Name = "Master",
                    Controller = "",
                    Action = @"[]",
                    Sequence = 4
                },
                
                // Chart of Accounts (Child of Master)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000007"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000006"),
                    Icon = "",
                    Name = "Chart of Accounts",
                    Controller = "ChartOfAccount",
                    Action = @"[""view"",""add"",""edit"",""delete""]",
                    Sequence = 1
                },
                
                // Customers (Child of Master)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000008"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000006"),
                    Icon = "",
                    Name = "Customers",
                    Controller = "Customer",
                    Action = @"[""view"",""add"",""edit"",""delete""]",
                    Sequence = 2
                },

                // Suppliers (Child of Master)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000009"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000006"),
                    Icon = "",
                    Name = "Suppliers",
                    Controller = "Supplier",
                    Action = @"[""view"",""add"",""edit"",""delete""]",
                    Sequence = 3
                },
                
                // Accounting (Parent)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    MenuParentId = null,
                    Icon = @"<i class=""ki-duotone ki-calculator fs-2""><span class=""path1""></span><span class=""path2""></span><span class=""path3""></span></i>",
                    Name = "Accounting",
                    Controller = "",
                    Action = @"[]",
                    Sequence = 5
                },
                
                // Journal Entry (Child of Accounting)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Icon = "",
                    Name = "Journal Entry",
                    Controller = "JournalEntry",
                    Action = @"[""view"",""add"",""edit"",""delete"",""post"",""reverse""]",
                    Sequence = 1
                },
                
                // General Ledger (Child of Accounting)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000012"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Icon = "",
                    Name = "General Ledger",
                    Controller = "GeneralLedger",
                    Action = @"[""view""]",
                    Sequence = 2
                },
                
                // Trial Balance (Child of Accounting)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000013"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Icon = "",
                    Name = "Trial Balance",
                    Controller = "TrialBalance",
                    Action = @"[""view""]",
                    Sequence = 3
                },
                
                // Financial Statements (Child of Accounting)
                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000014"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Icon = "",
                    Name = "Financial Statements",
                    Controller = "FinancialStatement",
                    Action = @"[""view""]",
                    Sequence = 4
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000015"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Icon = "",
                    Name = "Invoices",
                    Controller = "Invoice",
                    Action = @"[""view"",""add"",""edit"",""delete"",""post""]",
                    Sequence = 5
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000016"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Icon = "",
                    Name = "Payments & Receipts",
                    Controller = "Payment",
                    Action = @"[""view"",""add"",""edit"",""delete"",""post""]",
                    Sequence = 6
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000017"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Icon = "",
                    Name = "Aging Report",
                    Controller = "AgingReport",
                    Action = @"[""view""]",
                    Sequence = 7
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000030"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Icon = "",
                    Name = "Returns",
                    Controller = "Return",
                    Action = @"[""view"",""add"",""edit"",""delete"",""post""]",
                    Sequence = 8
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000040"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Icon = "",
                    Name = "Receivable & Payable",
                    Controller = "ReceivablePayable",
                    Action = @"[""view""]",
                    Sequence = 9
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000041"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Icon = "",
                    Name = "Taxes",
                    Controller = "Tax",
                    Action = @"[""view"",""add"",""edit"",""delete""]",
                    Sequence = 10
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000042"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Icon = "",
                    Name = "Fixed Assets",
                    Controller = "FixedAsset",
                    Action = @"[""view"",""add"",""edit"",""delete"",""post""]",
                    Sequence = 11
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000043"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Icon = "",
                    Name = "Year-End Closing",
                    Controller = "YearEndClosing",
                    Action = @"[""view"",""post""]",
                    Sequence = 12
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000044"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Icon = "",
                    Name = "Jurnal Memo / Penyesuaian",
                    Controller = "MemoJournal",
                    Action = @"[""view"",""add"",""edit"",""post"",""reverse""]",
                    Sequence = 13
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000031"),
                    MenuParentId = null,
                    Icon = @"<i class=""ki-duotone ki-cart fs-2""><span class=""path1""></span><span class=""path2""></span></i>",
                    Name = "Sales",
                    Controller = "",
                    Action = @"[]",
                    Sequence = 6
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000032"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000031"),
                    Icon = "",
                    Name = "Sales Quotation",
                    Controller = "SalesQuotation",
                    Action = @"[""view"",""add"",""edit"",""delete"",""post""]",
                    Sequence = 1
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000033"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000031"),
                    Icon = "",
                    Name = "Sales Order",
                    Controller = "SalesOrder",
                    Action = @"[""view"",""add"",""edit"",""delete"",""post""]",
                    Sequence = 2
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000034"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000031"),
                    Icon = "",
                    Name = "Delivery Order",
                    Controller = "DeliveryOrder",
                    Action = @"[""view"",""add"",""edit"",""delete"",""post""]",
                    Sequence = 3
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000018"),
                    MenuParentId = null,
                    Icon = @"<i class=""ki-duotone ki-delivery fs-2""><span class=""path1""></span><span class=""path2""></span><span class=""path3""></span></i>",
                    Name = "Purchasing",
                    Controller = "",
                    Action = @"[]",
                    Sequence = 7
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000035"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000018"),
                    Icon = "",
                    Name = "Purchase Request",
                    Controller = "PurchaseRequest",
                    Action = @"[""view"",""add"",""edit"",""delete"",""post""]",
                    Sequence = 1
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000019"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000018"),
                    Icon = "",
                    Name = "Purchase Orders",
                    Controller = "PurchaseOrder",
                    Action = @"[""view"",""add"",""edit"",""delete"",""post""]",
                    Sequence = 2
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000029"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000018"),
                    Icon = "",
                    Name = "Goods Received",
                    Controller = "GoodsReceipt",
                    Action = @"[""view"",""add"",""edit"",""delete"",""post""]",
                    Sequence = 3
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000020"),
                    MenuParentId = null,
                    Icon = @"<i class=""ki-duotone ki-package fs-2""><span class=""path1""></span><span class=""path2""></span><span class=""path3""></span></i>",
                    Name = "Inventory",
                    Controller = "",
                    Action = @"[]",
                    Sequence = 8
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000020"),
                    Icon = "",
                    Name = "Items",
                    Controller = "Inventory",
                    Action = @"[""view"",""add"",""edit"",""delete""]",
                    Sequence = 1
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000022"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000020"),
                    Icon = "",
                    Name = "Stock Card",
                    Controller = "Inventory",
                    Action = @"[""view""]",
                    Sequence = 2
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000023"),
                    MenuParentId = null,
                    Icon = @"<i class=""ki-duotone ki-shield-tick fs-2""><span class=""path1""></span><span class=""path2""></span></i>",
                    Name = "Approvals",
                    Controller = "Approval",
                    Action = @"[""view"",""post""]",
                    Sequence = 9
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000024"),
                    MenuParentId = null,
                    Icon = @"<i class=""ki-duotone ki-bank fs-2""><span class=""path1""></span><span class=""path2""></span><span class=""path3""></span></i>",
                    Name = "Kas & Bank",
                    Controller = "",
                    Action = @"[]",
                    Sequence = 10
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000025"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000024"),
                    Icon = "",
                    Name = "Cash & Bank Accounts",
                    Controller = "CashBank",
                    Action = @"[""view"",""add"",""edit"",""delete""]",
                    Sequence = 1
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000026"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000024"),
                    Icon = "",
                    Name = "Cash Bank Transfers",
                    Controller = "CashBank",
                    Action = @"[""view"",""add"",""edit"",""delete"",""post""]",
                    Sequence = 2
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000027"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000024"),
                    Icon = "",
                    Name = "Bank Reconciliation",
                    Controller = "CashBank",
                    Action = @"[""view"",""add"",""edit"",""delete"",""post""]",
                    Sequence = 3
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000028"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000020"),
                    Icon = "",
                    Name = "Stock Opname",
                    Controller = "StockOpname",
                    Action = @"[""view"",""add"",""edit"",""delete"",""post""]",
                    Sequence = 3
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000036"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000020"),
                    Icon = "",
                    Name = "Item Groups",
                    Controller = "ItemGroup",
                    Action = @"[""view"",""add"",""edit"",""delete""]",
                    Sequence = 4
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000037"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000020"),
                    Icon = "",
                    Name = "Units",
                    Controller = "ItemUnit",
                    Action = @"[""view"",""add"",""edit"",""delete""]",
                    Sequence = 5
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000038"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000020"),
                    Icon = "",
                    Name = "Stock Transfer",
                    Controller = "StockTransfer",
                    Action = @"[""view"",""add"",""edit"",""delete"",""post""]",
                    Sequence = 6
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000039"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000020"),
                    Icon = "",
                    Name = "Stock Minimum",
                    Controller = "StockMinimum",
                    Action = @"[""view""]",
                    Sequence = 7
                },

                new MenuEntity
                {
                    MenuId = Guid.Parse("00000000-0000-0000-0000-000000000045"),
                    MenuParentId = Guid.Parse("00000000-0000-0000-0000-000000000020"),
                    Icon = "",
                    Name = "Serial Number / Batch",
                    Controller = "SerialBatch",
                    Action = @"[""view"",""add"",""delete"",""post""]",
                    Sequence = 8
                }
            };
        }
    }
}
