# Backlog — Refactor CQRS (Clean Architecture)

Catatan kerja refactor dari pola **tradisional MVC + Service Layer** ke **Clean Architecture + CQRS (MediatR)** untuk AccuFlow.

> Branch kerja: `feature/cqrs-refactor`
> Database target baru: `AccuFlowCqrsDb` (LocalDB)
> Terakhir diperbarui: 2026-08-18
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
- [ ] **B14. Accounting > Aging Report** (User Manual §5.7) — `AgingReport` → AR/AP aging.
- [ ] **B15. Accounting > Returns** (User Manual §5.8) — `Return` → retur penjualan/pembelian.
- [ ] **B16. Accounting > Receivable & Payable** (User Manual §5.9) — `ReceivablePayable` → detail AR/AP per invoice.
- [ ] **B17. Accounting > Taxes** (User Manual §5.10) — `Tax` → CRUD pajak + laporan PPN.
- [ ] **B18. Accounting > Fixed Assets** (User Manual §5.11) — `FixedAsset` → daftar aset + penyusutan.
- [ ] **B19. Accounting > Year-End Closing** (User Manual §5.12) — `YearEndClosing` → tutup tahun + jurnal otomatis.
- [ ] **B20. Accounting > Jurnal Memo / Penyesuaian** (User Manual §5.13) — `MemoJournal` → memo/adjustment.
- [ ] **B21. Sales > Sales Quotation** (User Manual §6.1) — `SalesQuotation` → draft/approve, from quote → order.
- [ ] **B22. Sales > Sales Order** (User Manual §6.2) — `SalesOrder` → draft/approve, generate dari quotation.
- [ ] **B23. Sales > Delivery Order** (User Manual §6.3) — `DeliveryOrder` → draft/post (kurangi stok), convert ke sales invoice.
- [ ] **B24. Purchasing > Purchase Request** (User Manual §7.1) — `PurchaseRequest` → draft/approve, convert ke PO.
- [ ] **B25. Purchasing > Purchase Orders** (User Manual §7.2) — `PurchaseOrder` → draft/approve/convert/print.
- [ ] **B26. Purchasing > Goods Received / GRN** (User Manual §7.3) — `GoodsReceipt` → dari PO, update stok + jurnal.
- [ ] **B27. Inventory > Items** (User Manual §8.1) — `Inventory` → item list.
- [ ] **B28. Inventory > Stock Card** (User Manual §8.2) — `Inventory` → riwayat pergerakan stok.
- [ ] **B29. Inventory > Stock Opname** (User Manual §8.3) — `StockOpname` → stok fisik + adjustment.
- [ ] **B30. Inventory > Item Groups** (User Manual §8.4) — `ItemGroup` → CRUD grup barang.
- [ ] **B31. Inventory > Units** (User Manual §8.5) — `ItemUnit` → CRUD satuan.
- [ ] **B32. Inventory > Stock Transfer** (User Manual §8.6) — `StockTransfer` → mutasi antar gudang.
- [ ] **B33. Inventory > Stock Minimum** (User Manual §8.7) — `StockMinimum` → reorder point.
- [ ] **B34. Inventory > Serial Number / Batch** (User Manual §8.8) — `SerialBatch` → register lot/serial, consume.
- [ ] **B35. Approvals** (User Manual §9) — `Approval` → submit/approve/reject.
- [ ] **B36. Kas & Bank > Cash & Bank Accounts** (User Manual §10.1) — `CashBank` → list akun + saldo.
- [ ] **B37. Kas & Bank > Cash Bank Transfers** (User Manual §10.2) — `CashBank` → transfer antar akun.
- [ ] **B38. Kas & Bank > Bank Reconciliation** (User Manual §10.3) — `CashBank` → rekonsiliasi bank.
- [ ] **B39. HRM / Payroll > Data Karyawan** (User Manual §11.1) — `Payroll` → master karyawan.
- [ ] **B40. HRM / Payroll > Penggajian** (User Manual §11.2) — `Payroll` → buat + post payroll.
- [ ] **B41. Produksi > Bill of Material (BOM)** (User Manual §12.1) — `Production` → BOM.
- [ ] **B42. Produksi > Production Order** (User Manual §12.2) — `Production` → perintah produksi.

### C. Pengujian & Penyempurnaan

- [ ] **C1. Smoke test runtime** — login, CRUD COA, import template, laporan dasar.
- [ ] **C2. Uji multi-provider** (opsional) — coba `Database:Provider=Postgres` / `MySql` dengan connection string masing-masing.

---

## 3. Catatan / Risiko

- Migrasi EF Core **tidak lintas-provider** — dibutuhkan satu migration per engine (SqlServer/Postgres/MySql).
- Namespace lama (`AccuFlow.Entities.*`, `AccuFlow.Services.*`) akan banyak berubah; update referensi secara bertahap per modul agar tidak memecah build.
- `AccuFlowCqrsDb` masih kosong — migrasi pertama akan dibuat di sana, bukan menyentuh `AccuFlowDb`.
- Controller yang memakai `BaseController` / `IBaseService` perlu diarahkan ulang ke MediatR secara bertahap.
