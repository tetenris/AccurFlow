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

- [ ] **A1. Pindahkan entity** — `Entities/Entity` & `Entities/Abstractions` → `Domain/Entities` & `Domain/Common`; update semua namespace & referensi.
- [ ] **A2. Pindahkan DbContext & migrasi** — ke `Infrastructure/Persistence`; tambah design-time factory agar migrasi jalan lintas provider.
- [ ] **A3. Repository pattern** — `IRepository<T>` (generic) + implementasi EF; `IUnitOfWork`; daftarkan di `InfrastructureModule`.
- [ ] **A4. Behaviors MediatR** — `ValidationBehavior` & `LoggingBehavior` (opsional).
- [ ] **A5. Migrasi pertama ke `AccuFlowCqrsDb`** — verifikasi tabel + seeder berhasil saat startup.

### B. Modul Per Menu

- [ ] **B1. Login / Akun** (User Manual §1) — `Account` → Query/Command: Login, Logout, Change Password, Forgot Password, Lock/Unlock.
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
