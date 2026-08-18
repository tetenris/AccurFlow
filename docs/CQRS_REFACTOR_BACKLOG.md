# Backlog — Refactor CQRS (Clean Architecture)

Catatan kerja refactor dari pola **tradisional MVC + Service Layer** ke **Clean Architecture + CQRS (MediatR)** untuk AccuFlow.

> Branch kerja: `feature/cqrs-refactor`
> Database target baru: `AccuFlowCqrsDb` (LocalDB)
> Terakhir diperbarui: 2026-08-18 (tambah backlog D: cleanup service tersisa)
---

## 1. Yang Sudah Dikerjakan

- [x] **DB** — Database baru `AccuFlowCqrsDb` dibuat di `(localdb)\MSSQLLocalDB` (masih kosong, belum ada tabel).
- [x] **Konfigurasi** — `appsettings.json`: `DefaultConnection` diarahkan ke `AccuFlowCqrsDb` + tambah `Database:Provider = "SqlServer"`.
- [x] **Paket** — `MediatR` 12.2.0, `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.11, `Pomelo.EntityFrameworkCore.MySql` 8.0.2; EF Core naik `8.0.1` → `8.0.11`.
- [x] **Layer** — `Application/ApplicationModule.cs` (register MediatR), `Domain/Common/IEntity.cs`, `Infrastructure/InfrastructureModule.cs` (placeholder).
- [x] **Program.cs** — pemilihan provider via `Database:Provider` (SqlServer/Postgres/MySql), register `AddApplication()` + `AddInfrastructure()`; registrasi controller/auth tetap dipertahankan.
- [x] **Build** — `dotnet build` sukses (0 error; 26 warning nullability lama).

---

## 2. Backlog (Urut dari Database sampai Menu)

Penomoran mengikuti urutan modul di `docs/USER_MANUAL.md` (1. Login → 12. Produksi).

### A. Pondasi (Infrastruktur & Domain)

- [x] **A1. Pindahkan entity** — 45 file `Entities/Entity` → `Domain/Entities`; `BaseEntity` & `IEntity` → `Domain/Common`. Namespace: `AccuFlow.Entities.Entity` → `AccuFlow.Domain.Entities`, `AccuFlow.Entities.Abstractions` → `AccuFlow.Domain.Common`. Semua referensi (Controllers, Services, Models, Infrastructures, EntityConfigurations, Seeders, Migrations Designer) di-update. Build sukses (0 error). Folder `Entities/Abstractions` & `Entities/Entity` dihapus.
- [x] **A2. Pindahkan DbContext & migrasi** — `AppDbContext` → `Infrastructure/Persistence/AppDbContext.cs`; 57 file migrasi → `Infrastructure/Persistence/Migrations/`. Namespace: `AccuFlow.Entities.Context` → `AccuFlow.Infrastructure.Persistence`, `AccuFlow.Entities.Migrations` → `AccuFlow.Infrastructure.Persistence.Migrations`. Tambah `AppDbContextFactory` (design-time, dukung SqlServer/Postgres/MySql dari `Database:Provider`). Semua referensi (Program.cs, Controllers, Services, Seeders, Infrastructures) di-update (33 file). Build 0 error; `dotnet ef migrations list` & `database update` ke DB fresh terverifikasi (design-time factory jalan). Folder `Entities/Context` & `Entities/Migrations` dihapus.
- [x] **A3. Repository pattern** — `IRepository<T>` (generic, + `FirstOrDefaultWithIncludesAsync`) di `Application/Common/Interfaces`; implementasi EF `Repository<T>` & `UnitOfWork` di `Infrastructure/Persistence/Repositories`; didaftarkan di `InfrastructureModule`. Build 0 error.
- [ ] **A4. Behaviors MediatR** — (opsional; ditunda ke modul berikutnya).
- [x] **A5. Migrasi pertama ke `AccuFlowCqrsDb`** — `dotnet ef database update` sukses, 55 tabel dibuat. Seeder belum diverifikasi (dijalankan saat app startup).

### B. Modul Per Menu

> **PILOT diubah** → mulai dari Login/akun terlebih dahulu (bukan COA), sesuai keputusan 2026-08-18.

- [x] **B1. Login / Akun** (User Manual §1) — Selesai penuh. `AccountService` (`IAccountService` + `LoginResult` + `ValidateUser`, `RequestPasswordResetAsync`, `ChangePasswordAsync`) **DIHAPUS** dari `Services/` & `AppServiceCollection`. Semua aksi akun lewat MediatR:
  - Login → `LoginCommand` + `LoginCommandHandler` + `LoginResultDto` (validasi user, lock max 3x gagal, reset counter)
  - Change Password → `ChangePasswordCommand` + handler
  - Forgot Password → `ForgotPasswordCommand` + handler
  - Logout/Profile tetap di controller (tanpa service).
  Build 0 error; user percobaan `admin` / `Admin123!`.
- [x] **B2. Dashboard** (User Manual §2) — `HomeController` sudah lepas dari `AppDbContext` langsung → `GetDashboardSummaryQuery` + `GetDashboardSummaryQueryHandler` di `Application/Features/Dashboard` (agregat: Users, Roles, JournalEntries, Invoices, Payments, PurchaseOrders; pakai `IRepository<T>.Query()` + EF async; `IRepository` & `Repository<T>` ditambah `CountAsync`). Controller tinggal `IMediator.Send` + mapping ke `DashboardViewModel`. Build 0 error.
- [x] **B3. User Management > Users** (User Manual §3.1) — Selesai. `UserService` (`IUserService`) **DIHAPUS** dari `Services/` & `AppServiceCollection`. Semua aksi lewat MediatR di `Application/Features/Users`:
  - Query: `GetUsersDatatableQuery` (datatable + search/filter/sort/paging), `GetUserByIdQuery`, `GetUserRoleDropdownQuery`
  - Command: `CreateUserCommand`, `UpdateUserCommand`, `UnlockUserCommand` (reset ke `Qwerty@123` + expired), `DeleteUserCommand` (soft delete)
  - Helper baru `Application/Common/Helpers/SuperAdminCheck` (cek current user Super Administrator, dipakai handler User & Role)
  Build 0 error.
- [x] **B4. User Management > Roles** (User Manual §3.2) — Selesai. `RoleService` (`IRoleService`), `RoleMenuService` (`IRoleMenuService` + `IRoleMenuService.cs`) **DIHAPUS** dari `Services/` & `AppServiceCollection`. Semua lewat MediatR di `Application/Features/Roles`:
  - Query: `GetRolesDatatableQuery`, `GetRoleByIdQuery`, `GetRoleDropdownQuery`, `GetRoleMenuPermissionsQuery` (menu + permission matrix)
  - Command: `CreateRoleCommand`, `EditRoleCommand` (cek active users, role type change, deactivate guard), `DeleteRoleCommand` (soft delete role + role menus, cek user assigned), `SaveRoleMenuPermissionsCommand` (replace permission), `FixRoleTypeDataCommand`
  Build 0 error (25 warning).
  - **UI Role (2026-08-18)**: halaman Edit (pensil) hanya untuk edit info role (description + active); halaman `Permissions` (kunci) terpisah untuk manage access control (`/Role/Permissions`); **Delete role dihapus** (controller action + `DeleteRoleCommand` + handler + tombol).
- [x] **B5. Master > Chart of Accounts (PILOT)** (User Manual §4.1) — Selesai. `ChartOfAccountController` lepas dari `IChartOfAccountService` → MediatR (`ISender`). Semua lewat `Application/Features/ChartOfAccounts`:
  - Command: `CreateCoaCommand`, `UpdateCoaCommand`, `DeleteCoaCommand`, `ToggleCoaStatusCommand`, `ImportCoaCommand`
  - Query: `GetCoaDatatableQuery`, `GetCoaByIdQuery`, `GetCoaHierarchyQuery`, `GetCoaActiveAccountsQuery`, `GetCoaParentAccountsQuery`, `ValidateCoaCodeQuery`, `GenerateCoaCodeQuery`, `ExportCoaQuery`, `DownloadCoaTemplateQuery`
  - Handler port 1:1 dari `ChartOfAccountService` (logika sama; perbaiki bug rekalkulasi level di Update yang tidak pernah jalan di service lama). `ChartOfAccountService` lama **dipertahankan** (masih dipakai `JournalEntryController`, `MemoJournalController`, `GeneralLedgerController`). Build 0 error.
- [x] **B6. Master > Customers** (User Manual §4.2) — Selesai. `CustomerService` (`ICustomerService`) **DIHAPUS** dari `Services/` & `AppServiceCollection`. `CustomerController` → MediatR (`ISender`). Semua lewat `Application/Features/Customers`:
  - Command: `CreateCustomerCommand`, `UpdateCustomerCommand`, `DeleteCustomerCommand`, `ToggleCustomerStatusCommand`
  - Query: `GetCustomerDatatableQuery` (search/filter/sort/paging + resolve nama user), `GetCustomerByIdQuery`, `GetCustomerActiveQuery`, `ValidateCustomerCodeQuery`, `GenerateCustomerCodeQuery`, `ExportCustomerQuery`
  - Handler port 1:1 dari `CustomerService`. Build 0 error.
- [x] **B7. Master > Suppliers** (User Manual §4.3) — Selesai. `SupplierService` (`ISupplierService`) **DIHAPUS** dari `Services/` & `AppServiceCollection`. `SupplierController` → MediatR (`ISender`). Semua lewat `Application/Features/Suppliers`:
  - Command: `CreateSupplierCommand`, `UpdateSupplierCommand`, `DeleteSupplierCommand`, `ToggleSupplierStatusCommand`
  - Query: `GetSupplierDatatableQuery` (search/filter/sort/paging + resolve nama user), `GetSupplierByIdQuery`, `GetSupplierActiveQuery`, `ValidateSupplierCodeQuery`, `GenerateSupplierCodeQuery` (prefix `SUPP-`), `ExportSupplierQuery`
  - Handler port 1:1 dari `SupplierService`. Build 0 error.
- [x] **B8. Accounting > Journal Entry** (User Manual §5.1) — Selesai. `JournalEntryController` lepas dari `IJournalEntryService` → MediatR (`ISender`). Semua lewat `Application/Features/JournalEntries`:
  - Command: `CreateJournalCommand` (return id), `UpdateJournalCommand`, `DeleteJournalCommand`, `PostJournalCommand`, `ReverseJournalCommand` (swap debit/credit, status Reversed)
  - Query: `GetJournalDatatableQuery`, `GetJournalByIdQuery`, `GetJournalByNumberQuery`, `GenerateJournalNumberQuery`, `ExportJournalQuery`
  - Helper baru `Application/Common/Helpers/JournalEntryHelper` (generate nomor `JE/JA/JM-yyyyMMdd-NNNN` + validasi balanced/account/header, dipakai create/update/reverse/generate)
  - Handler port 1:1 dari `JournalEntryService`; properti komputasi (`CanEdit/CanPost/CanReverse` dll) tetap dari entity. `JournalEntryService` **dipertahankan** (masih dipakai Invoice, Payment, CashBank, Return, GoodsReceipt, FixedAsset, Payroll, Production, YearEndClosing, MemoJournal). Endpoint COA (GetAccountDropdown, CheckAccounts) tetap pakai `IChartOfAccountService`. Build 0 error.
- [x] **B9. Accounting > General Ledger** (User Manual §5.2) — Selesai. `GeneralLedgerController` lepas dari `IGeneralLedgerService` → MediatR (`ISender`). Semua lewat `Application/Features/GeneralLedgers`:
  - Query: `GetLedgerQuery` (buku besar per akun + opening/running/closing balance), `GetLedgerSummaryQuery` (ringkasan per akun), `GetAccountBalanceQuery` (saldo s.d. tanggal), `ExportLedgerQuery`, `ExportLedgerSummaryQuery`
  - Helper baru `Application/Common/Helpers/GeneralLedgerHelper` (kalkulasi saldo berdasarkan jenis akun, dipakai semua handler)
  - Handler port 1:1 dari `GeneralLedgerService` (read-only, tanpa command). `GeneralLedgerService` **dipertahankan** (masih dipakai TrialBalance, FinancialStatement, CashBank, YearEndClosing). Build 0 error.
- [x] **B10. Accounting > Trial Balance** (User Manual §5.3) — Selesai. `TrialBalanceService` (`ITrialBalanceService`) **DIHAPUS** dari `Services/` & `AppServiceCollection`. `TrialBalanceController` → MediatR (`ISender`). Semua lewat `Application/Features/TrialBalances`:
  - Query: `GetTrialBalanceQuery` (per akun, grouped per tipe, subtotal + total + difference + IsBalanced), `ExportTrialBalanceQuery` (Excel via NPOI, rekanan pakai handler ini)
  - Handler port 1:1 dari `TrialBalanceService`; saldo akun dihitung inline dari `JournalLine` (pakai `GeneralLedgerHelper.CalculateBalance`), tanpa dependensi `IGeneralLedgerService`. Build 0 error.
- [x] **B11. Accounting > Financial Statements** (User Manual §5.4) — Selesai. `FinancialStatementService` (`IFinancialStatementService`) **DIHAPUS** dari `Services/` & `AppServiceCollection`. `FinancialStatementController` → MediatR (`ISender`). Semua lewat `Application/Features/FinancialStatements`:
  - Query: `GetIncomeStatementQuery` (p/l per akun + subtotal + NetIncome), `GetBalanceSheetQuery` (Asset/Liability/Equity + balanced check), `GetCashFlowQuery` (operating/investing/financing + beginning/ending cash)
  - Export: `ExportIncomeStatementQuery`, `ExportBalanceSheetQuery`, `ExportCashFlowQuery` (Excel via NPOI)
  - Helper baru `Application/Common/Helpers/FinancialStatementHelper` (saldo & opening balance akun dari `JournalLine`, tanpa dependensi `IGeneralLedgerService`). Build 0 error.
- [x] **B12. Accounting > Invoices** (User Manual §5.5) — Selesai. `IInvoiceService`/`InvoiceService` (di `Services/BusinessModuleServices.cs`) **DIHAPUS** beserta registrasi DI. `InvoiceController` → MediatR (`ISender`). Semua lewat `Application/Features/Invoices`:
  - Query: `GetInvoiceDatatableQuery` (filter invoice type/status/customer/supplier/date/search), `GetInvoiceByIdQuery` (main detail + lines + journal)
  - Command: `CreateInvoiceCommand` (auto number `SI-`/`PI-`), `UpdateInvoiceCommand` (soft delete lines lama), `DeleteInvoiceCommand`, `PostInvoiceCommand` (auto jurnal via `CreateJournalCommand` + `PostJournalCommand` dari fitur JournalEntries — send berantai antar handler MediatR), `CancelInvoiceCommand`
  - Handler port 1:1 dari `InvoiceService`. Build 0 error.
- [x] **B13. Accounting > Payments & Receipts** (User Manual §5.6) — Selesai. `IPaymentService`/`PaymentService` (di `Services/BusinessModuleServices.cs`) **DIHAPUS** beserta registrasi DI. `PaymentController` → MediatR (`ISender`). Semua lewat `Application/Features/Payments`:
  - Query: `GetPaymentDatatableQuery` (filter payment type/status/date), `GetPaymentByIdQuery` (detail + alokasi invoice + jurnal)
  - Command: `CreatePaymentCommand` (auto number `PAY-`/`RCT-`), `UpdatePaymentCommand` (soft delete alokasi lama), `DeletePaymentCommand`, `PostPaymentCommand` (auto jurnal via `CreateJournalCommand` + `PostJournalCommand`, lalu update `PaidAmount` & status invoice per alokasi)
  - Handler port 1:1 dari `PaymentService`. Build 0 error.
- [x] **B14. Accounting > Aging Report** (User Manual §5.7) — Selesai. `IAgingReportService`/`AgingReportService` (di `Services/InventoryAndWorkflowServices.cs`) **DIHAPUS** beserta registrasi DI. `AgingReportController` → MediatR (`ISender`). Semua lewat `Application/Features/AgingReports`:
  - Query: `GetAgingReportQuery` (AR/AP, bucket Current/1-30/31-60/61-90/Over90 per partner, outstanding = Total - Paid)
  - Handler port 1:1 dari `AgingReportService`. Build 0 error.
- [x] **B15. Accounting > Returns** (User Manual §5.8) — Selesai. `IReturnService`/`ReturnService` (`Services/ReturnService.cs`) **DIHAPUS** beserta registrasi DI. `ReturnController` → MediatR (`ISender`). Semua lewat `Application/Features/Returns`:
  - Query: `GetReturnDatatableQuery` (filter return type/status/search), `GetReturnByIdQuery` (detail + line + jurnal), `GetReturnWarehousesQuery`, `GetReturnInvoicesQuery` (invoice posted yang masih punya sisa qty), `GetReturnInvoiceLinesQuery` (sisa qty per line)
  - Command: `CreateReturnCommand` (auto number `SR-`/`PR-`), `UpdateReturnCommand`, `PostReturnCommand` (validasi qty vs invoice, buat `StockMovement`, auto jurnal via `CreateJournalCommand` + `PostJournalCommand`), `DeleteReturnCommand`
  - Helper baru `Application/Features/Returns/Helpers/ReturnHelper` (akumulasi qty yang sudah di-return per invoice line, dipakai 3 handler). Handler port 1:1 dari `ReturnService`. Build 0 error.
- [x] **B16. Accounting > Receivable & Payable** (User Manual §5.9) — Selesai. `IReceivablePayableService`/`ReceivablePayableService` (di `Services/InventoryAndWorkflowServices.cs`) **DIHAPUS** beserta registrasi DI. `ReceivablePayableController` → MediatR (`ISender`). Semua lewat `Application/Features/ReceivablePayables`:
  - Query: `GetReceivablePayableQuery` (AR/AP per invoice, outstanding = Total - Paid, status Overdue/Open)
  - Handler port 1:1 dari `ReceivablePayableService`. Build 0 error.
- [x] **B17. Accounting > Taxes** (User Manual §5.10) — Selesai. `ITaxService`/`TaxService` (di `Services/InventoryAndWorkflowServices.cs`) **DIHAPUS** beserta registrasi DI. `TaxController` → MediatR (`ISender`). Semua lewat `Application/Features/Taxes`:
  - Query: `GetTaxDatatableQuery` (search/filter/paging), `GetTaxByIdQuery`, `GetVatReportQuery` (laporan PPN: DPP + Output/Input VAT dari invoice posted, anonim object `{ lines, summary }`)
  - Command: `CreateTaxCommand` (cek duplikasi kode), `UpdateTaxCommand`, `DeleteTaxCommand` (soft delete)
  - Handler port 1:1 dari `TaxService`. Build 0 error.
- [x] **B18. Accounting > Fixed Assets** (User Manual §5.11) — Selesai. `IFixedAssetService`/`FixedAssetService` (`Services/FixedAssetService.cs`) **DIHAPUS** beserta registrasi DI. `FixedAssetController` → MediatR (`ISender`). Semua lewat `Application/Features/FixedAssets`:
  - Query: `GetFixedAssetDatatableQuery` (filter category/status/search), `GetFixedAssetByIdQuery` (detail + history penyusutan)
  - Command: `CreateFixedAssetCommand` (auto number `FA-`), `UpdateFixedAssetCommand`, `DeleteFixedAssetCommand` (tolak jika ada riwayat penyusutan), `DepreciateFixedAssetCommand` (hitung bulan + cap, auto jurnal via `CreateJournalCommand` + `PostJournalCommand`, simpan `FixedAssetDepreciation`)
  - Handler port 1:1 dari `FixedAssetService`. Build 0 error.
- [x] **B19. Accounting > Year-End Closing** (User Manual §5.12) — Selesai. `IYearEndClosingService`/`YearEndClosingService` (`Services/YearEndClosingService.cs`) **DIHAPUS** beserta registrasi DI. `YearEndClosingController` → MediatR (`ISender`). Semua lewat `Application/Features/YearEndClosings`:
  - Query: `GetYearEndClosingPreviewQuery` (saldo per akun laba-rugi s.d. 31/12 via `GeneralLedgerHelper.CalculateBalance`), `GetYearEndClosingHistoryQuery`
  - Command: `CloseYearEndClosingCommand` (cekal fiscal year ganda, tutup akun revenue/expense → `RetainedEarnings`, auto jurnal via `CreateJournalCommand` + `PostJournalCommand`)
  - Handler port 1:1 dari `YearEndClosingService`. Build 0 error.
- [x] **B20. Accounting > Jurnal Memo / Penyesuaian** (User Manual §5.13) — Selesai. `MemoJournalController` → MediatR (`ISender`). Reuse fitur `JournalEntries` yang sudah CQRS: `GetJournalDatatableQuery`, `GetJournalByIdQuery`, `CreateJournalCommand`, `UpdateJournalCommand`, `PostJournalCommand`, `ReverseJournalCommand`, `GenerateJournalNumberQuery`; dropdown akun via `GetCoaActiveAccountsQuery`. `IJournalEntryService`/`IChartOfAccountService` **tetap** (masih dipakai controller/service lain). Build 0 error.
- [x] **B21. Sales > Sales Quotation** (User Manual §6.1) — Selesai. `IQuotationService`/`QuotationService` (`Services/SalesFlowServices.cs`) **DIHAPUS** beserta registrasi DI. `SalesQuotationController` → MediatR. Semua lewat `Application/Features/SalesQuotations`:
  - Query: `GetQuotationDatatableQuery`, `GetQuotationByIdQuery`, `GetApprovedQuotesQuery`
  - Command: `CreateQuotationCommand` (nomor `QT-xxxxx`), `UpdateQuotationCommand` (soft-delete line lama), `DeleteQuotationCommand`, `ApproveQuotationCommand`
  - Handler port 1:1 dari `QuotationService`. Build 0 error.
- [x] **B22. Sales > Sales Order** (User Manual §6.2) — Selesai. `ISalesOrderService`/`SalesOrderService` (`Services/SalesFlowServices.cs`) **DIHAPUS** beserta registrasi DI (`SalesFlowServices.cs` kini hanya berisi DeliveryOrderService). `SalesOrderController` → MediatR. Semua lewat `Application/Features/SalesOrders`:
  - Query: `GetOrderDatatableQuery`, `GetOrderByIdQuery`, `GetOrderQuoteByIdQuery` (quote approved utk generate order); `GetApprovedQuotes` reuse `GetApprovedQuotesQuery` (SalesQuotations)
  - Command: `CreateOrderCommand` (nomor `SO-xxxxx`), `UpdateOrderCommand`, `DeleteOrderCommand`, `ApproveOrderCommand`
  - Handler port 1:1 dari `SalesOrderService`. Build 0 error.
- [x] **B23. Sales > Delivery Order** (User Manual §6.3) — Selesai. `IDeliveryOrderService`/`DeliveryOrderService` (`Services/SalesFlowServices.cs`) **DIHAPUS** (file dihapus) beserta registrasi DI. `DeliveryOrderController` → MediatR. Semua lewat `Application/Features/DeliveryOrders`:
  - Query: `GetDeliveryDatatableQuery`, `GetDeliveryByIdQuery`, `GetDeliveryOrdersQuery` (SO approved dgn sisa qty > 0), `GetDeliveryOrderLinesQuery`
  - Command: `CreateDeliveryCommand` (nomor `DO-xxxxx`, validasi line milik SO approved), `UpdateDeliveryCommand`, `PostDeliveryCommand` (cek over-delivery + buat `StockMovement` keluar), `DeleteDeliveryCommand`, `ConvertDeliveryToInvoiceCommand` (auto buat invoice `SI-`)
  - Handler port 1:1 dari `DeliveryOrderService`. Build 0 error.
- [x] **B24. Purchasing > Purchase Request** (User Manual §7.1) — Selesai. `IPurchaseRequestService`/`PurchaseRequestService` (`Services/PurchaseRequestService.cs`) **DIHAPUS** beserta registrasi DI. `PurchaseRequestController` → MediatR. Semua lewat `Application/Features/PurchaseRequests`:
  - Query: `GetPurchaseRequestDatatableQuery`, `GetPurchaseRequestByIdQuery`
  - Command: `CreatePurchaseRequestCommand` (nomor `PR-xxxxx`, RequestedBy default nama user login), `UpdatePurchaseRequestCommand`, `DeletePurchaseRequestCommand`, `ApprovePurchaseRequestCommand`, `ConvertPurchaseRequestCommand` (auto buat PO `PO-`, status PR → Converted)
  - Handler port 1:1 dari `PurchaseRequestService`. Build 0 error.
- [x] **B25. Purchasing > Purchase Orders** (User Manual §7.2) — Selesai. `IPurchaseOrderService`/`PurchaseOrderService` (`Services/BusinessModuleServices.cs`) **DIHAPUS** (file dihapus) beserta registrasi DI. `PurchaseOrderController` → MediatR. Semua lewat `Application/Features/PurchaseOrders`:
  - Query: `GetPurchaseOrderDatatableQuery`, `GetPurchaseOrderByIdQuery` (juga dipakai action Print)
  - Command: `CreatePurchaseOrderCommand` (nomor `PO-xxxxx`), `UpdatePurchaseOrderCommand`, `DeletePurchaseOrderCommand` (soft-delete line), `ApprovePurchaseOrderCommand`, `ConvertPurchaseOrderToInvoiceCommand` (auto buat invoice `PI-`, status PO → Converted)
  - Handler port 1:1 dari `PurchaseOrderService`. Build 0 error.
- [x] **B26. Purchasing > Goods Received / GRN** (User Manual §7.3) — Selesai. `IGoodsReceiptService`/`GoodsReceiptService` (`Services/GoodsReceiptService.cs`) **DIHAPUS** beserta registrasi DI. `GoodsReceiptController` → MediatR. Semua lewat `Application/Features/GoodsReceipts`:
  - Query: `GetGoodsReceiptDatatableQuery`, `GetGoodsReceiptByIdQuery`, `GetGoodsReceiptWarehousesQuery`, `GetGoodsReceiptPurchaseOrdersQuery`, `GetGoodsReceiptPurchaseOrderLinesQuery`
  - Command: `CreateGoodsReceiptCommand` (nomor `GRN-xxxxx`), `UpdateGoodsReceiptCommand`, `DeleteGoodsReceiptCommand`, `PostGoodsReceiptCommand` (cek over-receipt, buat `StockMovement` masuk, auto jurnal beban inventory ↔ hutang via `CreateJournalCommand`+`PostJournalCommand`)
  - Handler port 1:1 dari `GoodsReceiptService`. Build 0 error.
- [x] **B27. Inventory > Items** (User Manual §8.1) — Selesai. `IInventoryService`/`InventoryService` (`Services/InventoryAndWorkflowServices.cs`) **DIHAPUS** beserta registrasi DI (`StockMinimumController` yang juga inject service → hanya `IBaseService`). `InventoryController` → MediatR. Semua lewat `Application/Features/Items`:
  - Query: `GetItemsDatatableQuery`, `GetActiveItemsQuery`, `GetStockCardQuery` (riwayat StockMovement), `GetStockMinimumQuery` (hitung stok & flag below reorder point)
  - Command: `CreateItemCommand` (cekal ItemCode ganda), `UpdateReorderPointCommand`
  - Handler port 1:1 dari `InventoryService`. Build 0 error.
- [x] **B28. Inventory > Stock Card** (User Manual §8.2) — Selesai. Ditangani bersama B27: `GetStockCardQuery` (`Application/Features/Items`) menggantikan `InventoryService.StockCard`. Build 0 error.
- [x] **B29. Inventory > Stock Opname** (User Manual §8.3) — Selesai. `IStockOpnameService`/`StockOpnameService` (`Services/StockOpnameService.cs`) **DIHAPUS** beserta registrasi DI. `StockOpnameController` → MediatR. Semua lewat `Application/Features/StockOpnames`:
  - Query: `GetStockOpnameDatatableQuery` (filter Status + Search, sort OpnameDate desc), `GetStockOpnameByIdQuery`, `GetStockOpnameWarehousesQuery`, `GetStockOpnameQuantitiesQuery` (stok sistem per gudang dari StockMovements)
  - Command: `CreateStockOpnameCommand` (no. `OPN-{D5}`), `UpdateStockOpnameCommand` (soft-delete line lama + add baru), `PostStockOpnameCommand` (buat StockMovement "Opname Adjustment"), `DeleteStockOpnameCommand`
  - Handler port 1:1 dari `StockOpnameService`. Build 0 error.
- [x] **B30. Inventory > Item Groups** (User Manual §8.4) — Selesai. `IItemGroupService`/`ItemGroupService` (`Services/InventoryAndWorkflowServices.cs`) **DIHAPUS** beserta registrasi DI. `ItemGroupController` → MediatR. Semua lewat `Application/Features/ItemGroups`:
  - Query: `GetItemGroupDatatableQuery` (termasuk `ItemCount`), `GetItemGroupByIdQuery`, `GetActiveItemGroupsQuery`
  - Command: `CreateItemGroupCommand` (cekal GroupCode ganda), `UpdateItemGroupCommand`, `DeleteItemGroupCommand` (tolak jika dipakai item)
  - Handler port 1:1 dari `ItemGroupService`. Build 0 error.
- [x] **B31. Inventory > Units** (User Manual §8.5) — Selesai. `IItemUnitService`/`ItemUnitService` (`Services/InventoryAndWorkflowServices.cs`) **DIHAPUS** beserta registrasi DI (file kini hanya berisi `IApprovalService` dan `IDocumentAttachmentService`). `ItemUnitController` → MediatR. Semua lewat `Application/Features/Units`:
  - Query: `GetUnitDatatableQuery` (search UnitCode/UnitName), `GetUnitByIdQuery`, `GetActiveUnitsQuery`
  - Command: `CreateUnitCommand` (cekal UnitCode ganda), `UpdateUnitCommand`, `DeleteUnitCommand`
  - Handler port 1:1 dari `ItemUnitService`. Build 0 error.
- [x] **B32. Inventory > Stock Transfer** (User Manual §8.6) — Selesai. `IStockTransferService`/`StockTransferService` (`Services/StockTransferService.cs`) **DIHAPUS** beserta registrasi DI. `StockTransferController` → MediatR. Semua lewat `Application/Features/StockTransfers`:
  - Query: `GetStockTransferDatatableQuery` (filter Status/WarehouseId/DateFrom/DateTo + Search), `GetStockTransferByIdQuery`, `GetStockTransferWarehousesQuery`
  - Command: `CreateStockTransferCommand` (no. `ST-{D5}`), `UpdateStockTransferCommand`, `PostStockTransferCommand` (cek stok tersedia → StockMovement "Stock Transfer Out/In"), `DeleteStockTransferCommand`
  - Handler port 1:1 dari `StockTransferService`. Build 0 error.
- [x] **B33. Inventory > Stock Minimum** (User Manual §8.7) — Selesai. Ditangani bersama B27: `GetStockMinimumQuery` (hitung stok aktual, flag `IsBelow` reorder point, filter `BelowOnly`) dan `UpdateReorderPointCommand` (`Application/Features/Items`). `StockMinimumController` hanya stub `Index`; datatable & update via `InventoryController` → MediatR. Tidak ada service terpisah. Build 0 error.
- [x] **B34. Inventory > Serial Number / Batch** (User Manual §8.8) — Selesai. `IStockBatchService`/`StockBatchService` (`Services/StockBatchService.cs`) **DIHAPUS** beserta registrasi DI. `SerialBatchController` → MediatR. Semua lewat `Application/Features/StockBatches`:
  - Query: `GetStockBatchDatatableQuery` (filter ItemId + Search; Status Available/Partial/Out), `GetStockBatchItemsQuery` (hanya item tipe Inventory)
  - Command: `CreateStockBatchCommand` (register batch/serial), `ConsumeStockBatchCommand` (cekal melebihi RemainingQuantity), `DeleteStockBatchCommand` (tolak batch sudah terpakai)
  - Handler port 1:1 dari `StockBatchService`. Build 0 error.
- [x] **B35. Approvals** (User Manual §9) — Selesai. `IApprovalService`/`ApprovalService` (`Services/InventoryAndWorkflowServices.cs`) **DIHAPUS** beserta registrasi DI (file kini hanya berisi `IDocumentAttachmentService`). `ApprovalController` → MediatR (`ISender`). Semua lewat `Application/Features/Approvals`:
  - Query: `GetApprovalDatatableQuery` (filter DocumentType/Status + paging, resolve nama requester)
  - Command: `SubmitApprovalCommand` (status Pending, CurrentApproverId), `ApproveApprovalCommand` (status Approved + riwayat), `RejectApprovalCommand` (status Rejected + riwayat)
  - Handler port 1:1 dari `ApprovalService`. Build 0 error.
- [x] **B36. Kas & Bank > Cash & Bank Accounts** (User Manual §10.1) — Selesai. `CashBankController` aksi akun (Accounts/GetCashAccounts/GetBankAccounts) lepas dari `ICashBankService` → MediatR (`ISender`). Semua lewat `Application/Features/CashBankAccounts`:
  - Query: `GetCashBankAccountsQuery` (filter `AccountUsage` 1=cash / 2=bank, non-header, active; saldo per akun dihitung ulang via `GetAccountBalanceQuery` dari fitur GeneralLedgers — reuse send berantai antar handler)
  - Handler port 1:1 dari `CashBankService.GetAccountsAsync` (perbaiki bug: dulu pakai `.Result` sync-over-async di controller, sekarang async penuh). Aksi transfers/reconciliation masih pakai `ICashBankService` (B37/B38). Service tetap, method akun (**GetCashAccountsAsync/GetBankAccountsAsync**) dihapus dari interface & class beserta dependensi `IGeneralLedgerService`/`ILogger`. Build 0 error.
- [x] **B37. Kas & Bank > Cash Bank Transfers** (User Manual §10.2) — Selesai. `CashBankController` aksi transfer (DatatableTransfers/GetTransferDetail/CreateTransfer/UpdateTransfer/PostTransfer/DeleteTransfer) lepas dari `ICashBankService` → MediatR. Semua lewat `Application/Features/CashBankTransfers`:
  - Query: `GetTransferDatatableQuery` (filter Status/DateFrom/DateTo + search, paging), `GetTransferDetailQuery` (termasuk JournalNumber + CanEdit/CanDelete/CanPost)
  - Command: `CreateTransferCommand` (no. `TRF-{D5}`, validasi akun cash/bank), `UpdateTransferCommand`, `PostTransferCommand` (auto jurnal via `CreateJournalCommand` + `PostJournalCommand`), `DeleteTransferCommand`
  - Handler port 1:1 dari `CashBankService`. Method transfer dihapus dari interface & class service (kini hanya reconciliation); dependensi `IJournalEntryService` + using `Models.JournalEntry` dihapus. Build 0 error.
- [x] **B38. Kas & Bank > Bank Reconciliation** (User Manual §10.3) — Selesai. `ICashBankService`/`CashBankService` (`Services/CashBankService.cs`) **DIHAPUS** beserta registrasi DI. `CashBankController` → MediatR penuh (`ISender`). Semua lewat `Application/Features/BankReconciliations`:
  - Query: `GetReconciliationDatatableQuery` (filter Status + search, paging, LineCount/ClearedCount), `GetReconciliationDetailQuery` (detail + lines + CanEdit/CanDelete/CanPost), `GetBankStatementQuery` (mutasi GL posted s.d. tanggal)
  - Command: `CreateReconciliationCommand` (no. `RCN-{D5}`), `UpdateReconciliationCommand` (soft-delete line lama), `PostReconciliationCommand` (cek keseimbangan GL + cleared − float = statement), `DeleteReconciliationCommand`
  - Handler port 1:1 dari `CashBankService`. Build 0 error. Modul Kas & Bank (B36–B38) tuntas.
- [x] **B39. HRM / Payroll > Data Karyawan** (User Manual §11.1) — Selesai. `PayrollController` aksi employee (EmployeeDatatable/CreateEmployee/UpdateEmployee/DeleteEmployee) lepas dari `IPayrollService` → MediatR. Semua lewat `Application/Features/Employees`:
  - Query: `GetEmployeeDatatableQuery` (search EmployeeCode/FullName/Position/Department + paging)
  - Command: `CreateEmployeeCommand` (cekal EmployeeCode ganda), `UpdateEmployeeCommand` (cekal ganda kecuali diri sendiri), `DeleteEmployeeCommand` (tolak jika sudah ada payroll record)
  - Handler port 1:1 dari `PayrollService`. Method employee dihapus dari interface & class service (kini hanya payroll). Build 0 error.
- [x] **B40. HRM / Payroll > Penggajian** (User Manual §11.2) — Selesai. `IPayrollService`/`PayrollService` (`Services/PayrollService.cs`) **DIHAPUS** beserta registrasi DI. `PayrollController` → MediatR penuh (`ISender`). Semua lewat `Application/Features/Payrolls`:
  - Query: `GetPayrollDatatableQuery` (search PayrollNumber/Notes, sort period, LineCount + JournalNumber), `GetPayrollByIdQuery` (detail + line pegawai)
  - Command: `CreatePayrollCommand` (no. `PR-yyyymm-NNNN`, cek period ganda, tarik semua pegawai aktif), `PostPayrollCommand` (auto jurnal beban gaji ↔ hutang gaji via `CreateJournalCommand` + `PostJournalCommand`), `DeletePayrollCommand` (soft delete)
  - Handler port 1:1 dari `PayrollService`. Build 0 error. Modul HRM/Payroll (B39–B40) tuntas.
- [x] **B41. Produksi > Bill of Material (BOM)** (User Manual §12.1) — Selesai. `ProductionController` aksi BOM (BomDatatable/GetBomById/Items/Warehouses/Boms/CreateBom/DeleteBom) lepas dari `IProductionService` → MediatR. Semua lewat `Application/Features/BillOfMaterials`:
  - Query: `GetBomDatatableQuery` (search BomNumber/finished item + paging, LineCount), `GetBomByIdQuery` (detail + line komponen), `GetBomsQuery` (BOM aktif utk dropdown PO)
  - Command: `CreateBomCommand` (no. `BOM-{D5}`, cek komponen = finished item), `DeleteBomCommand` (tolak jika sudah ada production order posted)
  - Review dropdown reuse `GetActiveItemsQuery` (Items, B27) + `GetWarehousesQuery` baru (`Application/Features/Warehouses`, dipakai juga B42). Method BOM dihapus dari `ProductionService` (kini hanya production order). Build 0 error.
- [x] **B42. Produksi > Production Order** (User Manual §12.2) — Selesai. `IProductionService`/`ProductionService` (`Services/ProductionService.cs`) **DIHAPUS** beserta registrasi DI. `ProductionController` → MediatR penuh (`ISender`). Semua lewat `Application/Features/ProductionOrders`:
  - Query: `GetProductionOrderDatatableQuery` (search no. order/finished item, sort ProductionDate, LineCount + JournalNumber), `GetProductionOrderByIdQuery` (detail + line komponen)
  - Command: `CreateProductionOrderCommand` (no. `PRD-{D5}`, wajib BOM, tarik line komponen `qty BOM × qty produksi`), `PostProductionOrderCommand` (cek stok komponen vs `QuantityRequired`, buat `StockMovement` Production Consumption/Output, auto jurnal WIP via `CreateJournalCommand`+`PostJournalCommand`, reuse `GetWarehousesQuery`), `DeleteProductionOrderCommand` (soft delete)
  - Handler port 1:1 dari `ProductionService`. Build 0 error. **Seluruh backlog B (B1–B42) TUNTAS.**

### C. Pengujian & Penyempurnaan

- [ ] **C1. Smoke test runtime** — login, CRUD COA, import template, laporan dasar.
- [ ] **C2. Uji multi-provider** (opsional) — coba `Database:Provider=Postgres` / `MySql` dengan connection string masing-masing.

### D. Bersihkan Service Lama yang Tersisa → CQRS (pasca B1–B42)

Hasil verifikasi `Services/` (2026-08-18): hanya ada 6 file service tersisa. Rincian pemakaian aktual & rencana migrasi:

1. [x] **`IChartOfAccountService`/`ChartOfAccountService` — migrasi, lalu hapus.** Selesai 2026-08-18. Tiga titik dipakai alihkan ke query CQRS:
   - `JournalEntryController.GetAccountDropdown()` → `GetCoaDetailAccountsQuery` (query **baru**, port 1:1 dari `GetDetailAccountsAsync` — filter `!IsHeader && IsActive`; tidak bisa pakai `GetCoaActiveAccountsQuery` karena itu menyertakan header)
   - `JournalEntryController.CheckAccounts()` → `GetCoaDatatableQuery` + `GetCoaDetailAccountsQuery`
   - `GeneralLedgerController.ExportLedger()` → `GetCoaByIdQuery`
   - `Services/ChartOfAccountService.cs` **DIHAPUS** + registrasi DI (`AppServiceCollection.cs`) dihapus. Build 0 error.
2. [ ] **`IMenuService`/`MenuService` — migrasi ke CQRS.** Dipakai sidebar via `@inject` di `Views/Shared/_Sidebar.cshtml` & `_Sidebar_Simple.cshtml` (`GetMenuHierarchyAsync` / `GetMenuHierarchyByRoleAsync`). Rencana: `Application/Features/Menus` → `GetMenuHierarchyQuery` (bisa filter role), view injeksi `ISender`; hapus `MenuService.cs` + `IMenuService.cs` + DI.
3. [ ] **`IDocumentAttachmentService`/`DocumentAttachmentService` — migrasi ke CQRS.** Dipakai `DocumentAttachmentController` (`Datatable`, `GetByDocument`, `Upload` [IFormFile + config `Storage:*`], `Download`, `Delete`). Rencana: `Application/Features/DocumentAttachments` → `GetAttachmentDatatableQuery`, `GetAttachmentsByDocumentQuery`, `UploadAttachmentCommand`, `GetAttachmentFileQuery`, `DeleteAttachmentCommand`; controller → `ISender`; hapus `InventoryAndWorkflowServices.cs` + DI.
4. [ ] **Hapus `IJournalEntryService`/`JournalEntryService` (file `JournalEntryService.cs`).** Setelah semua pemakai bermigrasi (Invoice B12, Payment B13, CashBank B36–B38, Return B15, GoodsReceipt B26, FixedAsset B18, Payroll B40, Production B42, YearEndClosing B19, MemoJournal B20), tidak ada controller/service yang meng-inject lagi (grep terverifikasi). Catatan lama B8/B20 yang menulis "dipertahankan karena masih dipakai …" **tidak berlaku lagi**. Hapus file + DI (`AppServiceCollection.cs`).
5. [ ] **Hapus `IGeneralLedgerService`/`GeneralLedgerService` (2 file).** Setelah TrialBalance B10, FinancialStatement B11, CashBank B36, YearEndClosing B19 memakai helper/query sendiri, tidak ada yang meng-inject (grep terverifikasi). Hapus 2 file + DI (`AppServiceCollection.cs`).
6. [ ] **`BaseService`/`IBaseService` — dipertahankan (foundation).** Dipakai seluruh ~35 controller via `BaseController` (konstruktor). Migrasi total menyentuh `BaseController` + semua controller → di luar cakupan backlog D, dievaluasi terpisah.

---

## 3. Catatan / Risiko

- Migrasi EF Core **tidak lintas-provider** — dibutuhkan satu migration per engine (SqlServer/Postgres/MySql).
- Namespace lama (`AccuFlow.Entities.*`, `AccuFlow.Services.*`) akan banyak berubah; update referensi secara bertahap per modul agar tidak memecah build.
- `AccuFlowCqrsDb` masih kosong — migrasi pertama akan dibuat di sana, bukan menyentuh `AccuFlowDb`.
- Controller yang memakai `BaseController` / `IBaseService` perlu diarahkan ulang ke MediatR secara bertahap.
