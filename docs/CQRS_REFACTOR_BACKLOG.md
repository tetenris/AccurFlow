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
- [ ] **B2. Dashboard** (User Manual §2) — `Home` → Query: ringkasan metrik keuangan, journal posting health, recent activity, quick actions.
- [ ] **B3. User Management > Users** (User Manual §3.1) — `User` → CRUD, Unlock/Reset Password, Audit Trail.
- [ ] **B4. User Management > Roles** (User Manual §3.2) — `Role` → CRUD + permission matrix.
- [ ] **B5. Master > Chart of Accounts (PILOT)** (User Manual §4.1) — `ChartOfAccount` → Command: Create, Update, Delete, ToggleStatus, Import; Query: Paginated, GetById, GetParents, GetHierarchy, GetActive, GenerateCode, ValidateCode; plus DownloadTemplate.
- [ ] **B6. Master > Customers** (User Manual §4.2) — `Customer` → CRUD, ToggleStatus, Export.
- [ ] **B7. Master > Suppliers** (User Manual §4.3) — `Supplier` → CRUD, ToggleStatus, Export.
- [ ] **B8. Accounting > Journal Entry** (User Manual §5.1) — `JournalEntry` → draft/post/reverse.
- [ ] **B9. Accounting > General Ledger** (User Manual §5.2) — `GeneralLedger` → summary + ledger.
- [ ] **B10. Accounting > Trial Balance** (User Manual §5.3) — `TrialBalance` → generate + export.
- [ ] **B11. Accounting > Financial Statements** (User Manual §5.4) — `FinancialStatement` → income/balance/cash flow.
- [ ] **B12. Accounting > Invoices** (User Manual §5.5) — `Invoice` → draft/edit/post/cancel/print + attachment.
- [ ] **B13. Accounting > Payments & Receipts** (User Manual §5.6) — `Payment` → draft/edit/post/print + alokasi invoice.
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
