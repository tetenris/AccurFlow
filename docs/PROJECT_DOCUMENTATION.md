# AccuFlow — Dokumentasi Proyek

AccuFlow adalah aplikasi **ASP.NET Core 8 MVC** untuk sistem akuntansi (Accounting System). Dokumen ini menggabungkan catatan proyek, cara menjalankan aplikasi, status fitur, perbandingan menu dengan Accurate, dan roadmap pengembangan.

> Status: repository siap untuk GitHub; branch pengembangan sudah lengkap (41 menu terhubung, seluruh backlog referensi Accurate selesai).

---

## 1. Gambaran Umum

- Project bernama **AccuFlow**, aplikasi ASP.NET Core 8 MVC untuk sistem akuntansi.
- Database memakai SQL Server via Entity Framework Core; connection default di `appsettings.json`.
- UI memakai Metronic/Bootstrap dengan Razor Views di `Views/`.
- Background job memakai Hangfire, dashboard di `/hangfire`.
- Export Excel memakai NPOI; PDF dependency DinkToPdf sudah terpasang (print ready via browser Print → Save as PDF).

## 2. Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **UI Template**: Metronic (Bootstrap 5)
- **Background Jobs**: Hangfire
- **PDF Generation**: DinkToPdf (belum dipakai untuk export, print via browser)
- **Excel**: NPOI

---

## 3. Cara Menjalankan

### 3.1 Setup Database & Jalankan

```powershell
dotnet restore
# Edit appsettings.json -> ConnectionStrings.DefaultConnection
dotnet ef migrations add InitialCreate   # wajib untuk project baru
dotnet ef database update                # jika perlu
dotnet run
```

Buka browser: `https://localhost:5001`

> `Program.cs` menjalankan `Database.MigrateAsync()` otomatis saat aplikasi start, sehingga migration diterapkan sendiri.

Seeder otomatis berjalan saat startup: roles, users, chart of accounts, customers, suppliers, menus.

**User seed default** (lihat `Entities/Seeders/UserSeed.cs`):
- `admin / Admin123!` (role Super Administrator)
- `administrator / Admin123!` (role Administrator)
- `accountant / Accountant123!` (role Accountant)

Auth memakai cookie login di `/Account/Login` (BCrypt + claim selaras dengan `CurrentUserService`).

> Project ini awalnya di-generate dari template `KMSI.SuperApps.Ews` via skrip scaffold sekali pakai (sudah dihapus); sekarang project berada langsung di root repository.

---

## 4. Struktur Project

```
AccuFlow/
├── Controllers/          # MVC Controllers (Controller tipis)
├── Models/               # View Models & DTOs
│   ├── Account/  ChartOfAccount/  JournalEntry/
│   ├── Invoice/  PurchaseOrder/   Inventory/  Report/  ...
├── Services/             # Business Logic Services
├── Entities/             # Database Entities
│   ├── Context/          # AppDbContext.cs (daftarkan DbSet di sini)
│   ├── Entity/  EntityConfigurations/  Migrations/
├── Views/                # Razor Views (Metronic/Bootstrap 5)
│   ├── Shared/  Home/  ChartOfAccount/  JournalEntry/ ...
├── wwwroot/              # Static Files (Metronic Template: assets/css/js/theme)
├── Extentions/           # Extension Methods
├── Helpers/              # Helper Classes
├── Infrastructures/      # Infrastructure Services (DI di AppServiceCollection.cs)
└── Job/                  # Background Jobs (Hangfire)
```

---

## 5. Arsitektur & Pola Pengembangan

- Controller MVC di `Controllers/`; business logic di `Services/`.
- Entity & konfigurasi EF di `Entities/Entity` & `Entities/EntityConfigurations`.
- Request/view model di `Models/`.
- Dependency injection didaftarkan di `Infrastructures/AppServiceCollection.cs`.
- Startup pipeline di `Program.cs`.

Pola yang wajib diikuti:
1. Controller tipis, logic di Service, model request/view di `Models`.
2. Query EF umumnya memfilter soft delete dengan `IsDeleted`.
3. Untuk fitur baru, daftarkan service di `Infrastructures/AppServiceCollection.cs`.
4. Untuk entity baru, tambahkan DbSet di `Entities/Context/AppDbContext.cs` dan konfigurasi di `Entities/EntityConfigurations` bila perlu.

Namespace sisa template `KomatsuERP` sudah dirapikan ke `AccuFlow`.

---

## 6. Fitur Utama

### Accounting Core
- **Chart of Accounts** — master akun (CRUD, hierarchy, export)
- **Journal Entry** — draft/post/reverse (nomor `JE-`)
- **General Ledger** — buku besar + summary
- **Trial Balance** — generate + export
- **Financial Statements** — income statement, balance sheet, cash flow

### Transaksi
- **Invoice/Billing** — draft/edit/post/cancel/print (`SI-`)
- **Payment/Receipt** — alokasi invoice, post payment
- **Purchase Order** — draft/approve/convert/print (`PO-`)
- **Purchase Request** — draft/approve, convert ke PO (`PR-`)
- **Returns** — retur penjualan/pembelian

### Alur Penjualan
- **Sales Quotation** — draft/approve, from quote → order
- **Sales Order** — draft/approve, generate dari quotation
- **Delivery Order** — draft/post (kurangi stok), convert ke sales invoice

### Alur Pembelian
- **Goods Received (GRN)** — dari PO, update stok + jurnal persediaan

### Inventory
- **Items** + **Item Groups** + **Units**
- **Stock Card**, **Stock Opname**, **Stock Transfer**, **Stock Minimum**
- **Serial Number / Batch** — register lot, konsumsi, sisa stok

### Kas & Bank
- **Cash & Bank Accounts**, **Cash Bank Transfers**, **Bank Reconciliation**

### Piutang & Utang
- **Aging Report** (AR/AP aging), **Receivable & Payable**

### Pajak
- **Taxes** — CRUD tarif PPN 11% + laporan PPN keluaran/masukan

### Aktiva Tetap
- **Fixed Assets** — daftar aset + penyusutan garis lurus (jurnal otomatis)

### Akuntansi Lanjutan
- **Year-End Closing** — preview P&L → jurnal otomatis ke Retained Earnings
- **Jurnal Memo / Penyesuaian** — tipe Memo (`JM-`) / Adjustment (`JA-`)

### HRM / Payroll
- **Payroll** — master karyawan + penggajian (draft → post jurnal Salary/Payroll Payable)

### Produksi
- **Production** — Bill of Material + Production Order (konsumsi komponen, output barang jadi)

### Utility / Setting
- **User Management** (Users, Roles + permission matrix)
- **Approvals** — submit/approve/reject
- **Document Attachment** — upload/download/delete
- **Database Seeding** (khusus admin dev)

---

## 7. Perbandingan Menu AccuFlow vs Accurate + Backlog

### 7.1 Daftar Menu yang Sudah Ada (41 menu terhubung)

| No | Menu | Controller | Keterangan |
|----|------|-----------|-------------|
| 1 | Dashboard | Home | + |
| 2 | Database Seeding | Seed | khusus admin (dev) |
| 3 | User Management > Users | User | CRUD + unlock |
| 4 | User Management > Roles | Role | CRUD + permission matrix |
| 5 | Master > Chart of Accounts | ChartOfAccount | CRUD, hierarchy, export |
| 6 | Master > Customers | Customer | CRUD, status, export |
| 7 | Master > Suppliers | Supplier | CRUD, status, export |
| 8 | Accounting > Journal Entry | JournalEntry | draft/post/reverse |
| 9 | Accounting > General Ledger | GeneralLedger | ledger, summary |
| 10 | Accounting > Trial Balance | TrialBalance | generate + export |
| 11 | Accounting > Financial Statements | FinancialStatement | income/balance/cash flow |
| 12 | Accounting > Invoices | Invoice | draft/edit/post/cancel/print |
| 13 | Accounting > Payments & Receipts | Payment | draft/edit/post/print |
| 14 | Accounting > Aging Report | AgingReport | AR/AP aging |
| 15 | Purchasing > Purchase Request | PurchaseRequest | draft/approve, convert ke PO |
| 16 | Purchasing > Purchase Orders | PurchaseOrder | draft/approve/convert/print |
| 17 | Inventory > Items | Inventory | create item, tampil group |
| 18 | Inventory > Stock Card | Inventory | stock movement history |
| 19 | Approvals | Approval | submit/approve/reject |
| 20 | Document Attachment (via modul) | DocumentAttachment | upload/download/delete |
| 21 | Inventory > Stock Opname | StockOpname | CRUD + post (input hasil, selisih, stock adjustment) |
| 22 | Kas & Bank > Cash & Bank Accounts | CashBank | list akun + saldo |
| 23 | Kas & Bank > Cash Bank Transfers | CashBank | draft/edit/post antar akun |
| 24 | Kas & Bank > Bank Reconciliation | CashBank | draft/edit/post |
| 25 | Purchasing > Goods Received | GoodsReceipt | GRN dari PO, update stok + jurnal persediaan |
| 26 | Accounting > Returns | Return | retur penjualan/pembelian, kurangi stok + piutang/utang |
| 27 | Penjualan > Sales Quotation | SalesQuotation | draft/approve/edit/delete, from quote → order |
| 28 | Penjualan > Sales Order | SalesOrder | draft/approve, generate dari quotation |
| 29 | Penjualan > Delivery Order | DeliveryOrder | draft/post (kurangi stok), convert ke sales invoice |
| 30 | Inventory > Item Groups | ItemGroup | CRUD group/jenis barang, pakai di item |
| 31 | Inventory > Units | ItemUnit | CRUD satuan (PCS/BOX/SET, dll.) |
| 32 | Inventory > Stock Transfer | StockTransfer | mutasi stok antar gudang, draft/edit/post |
| 33 | Inventory > Stock Minimum | StockMinimum | reorder point, notifikasi stok di bawah minimum |
| 34 | Accounting > Receivable & Payable | ReceivablePayable | daftar detail piutang/utang per customer/supplier |
| 35 | Accounting > Taxes | Tax | CRUD pajak (PPN 11%) + tab laporan PPN keluaran/masukan |
| 36 | Accounting > Fixed Assets | FixedAsset | daftar aktiva tetap + penyusutan (jurnal otomatis) |
| 37 | Accounting > Year-End Closing | YearEndClosing | tutup tahun fiskal: preview saldo P&L → jurnal otomatis ke Retained Earnings |
| 38 | Accounting > Jurnal Memo / Penyesuaian | MemoJournal | jurnal manual berjenis Memo (JM-) / Adjustment (JA-) |
| 39 | Inventory > Serial Number / Batch | SerialBatch | register batch/lot/serial per item, konsumsi, sisa stok batch |
| 40 | HRM / Payroll > Payroll | Payroll | master karyawan + penggajian (draft → post jurnal gaji otomatis) |
| 41 | Produksi > Production | Production | Bill of Material + Production Order (konsumsi komponen, output barang jadi) |

Sudah ada di kode tapi belum jadi menu/UI:
- Entity `Tax` + seed PPN 11%
- Entity warehouse (`Warehouse`) + seed gudang awal
- Entity approval request/history

### 7.2 Backlog

**Backlog sudah habis** — semua modul referensi Accurate telah diimplementasikan. Sisa pengembangan berikutnya (opsional/penyempurnaan) bisa mengikuti prioritas operasional, misalnya: purchase invoice & supplier payment terpisah, retur pola Accurate penuh, laporan kas/bank, payroll dengan tunjangan/potongan per karyawan, bill of material multi-level, dan integration stock batch ke transaksi.

### 7.3 Catatan Kondisi Saat Ini

- Semua role (kecuali Super Administrator & Administrator) belum punya seed permission menu (`SeedRoleMenus` hanya mengisi 2 role sistem). Backlog permission per role perlu dibuat agar menu tampil sesuai peran.
- Sebagian modul bisnis (Invoice, Payment, PO) menggabungkan jenis sales & purchasing dalam satu tempat; jika ingin persis Accurate sebaiknya dipisah ke menu Penjualan dan Pembelian.

---

## 8. Status Implementasi

### 8.1 Sudah Ada
- Core app: ASP.NET Core 8 MVC, EF Core SQL Server, migration otomatis, DI, layout Metronic, cookie auth.
- Master data: `Role`, `User`, `Menu`, `RoleMenu`, `ChartOfAccount`, `Customer`, `Supplier`.
- Accounting core: `JournalEntry`, `JournalLine`, posting, reversal, soft delete.
- Report accounting: `GeneralLedger`, `TrialBalance`, `FinancialStatement` (income, balance, cash flow).
- Export Excel: chart of accounts, customer, supplier, journal entry, general ledger, trial balance, financial statements.
- Alur penjualan: `SalesQuotation` / `SalesOrder` / `DeliveryOrder` (post kurangi stok, convert ke invoice).
- Alur pembelian: `PurchaseRequest` (convert ke PO sesuai supplier & tanggal).
- Master inventory: `ItemGroup` & `Unit` (CRUD + seed); `Item` ber-relasi ke group.
- Seeder: roles, users, chart of accounts, customers, suppliers, menus.
- Menu **Database Seeding** dihapus dari sidebar (dev-only, route `/Seed` tetap ada & dibatasi Super Administrator); `Seeder.SeedMenu` kini soft-delete menu yang tidak ada di seed.
- Build status: `dotnet build AccuFlow.sln` berhasil compile (masih ada warning nullability).

### 8.2 Roadmap Bisnis (kronologi modul)

- **Invoice/Billing**: entity, migration, service, controller, menu, view list, JS datatable, create draft, post, cancel.
- **Payment/Receipt**: entity, migration, service, controller, menu, view list, JS datatable, allocation invoice, post payment.
- **AR/AP Aging**: report dari invoice outstanding (bucket current, 1-30, 31-60, 61-90, >90 hari).
- **Purchase Order**: entity, migration, service, controller, menu, view list, approve, convert to purchase invoice.
- **Inventory**: item, warehouse, stock movement, stock opname entity, item list, stock card list, seed warehouse awal.
- **Approval Workflow**: approval request/history entity, service, controller, view list, approve/reject dasar.
- **Document Attachment**: entity dan service datatable dasar; upload/download file fisik belum diimplementasikan.
- **Tax Management**: entity dan seed PPN 11%; UI CRUD tax sudah dibuat di modul Taxes (angka 35).
- **Kas & Bank**: modul lengkap (`Cash & Bank Accounts`, `Cash Bank Transfers`, `Bank Reconciliation`). Kolom `AccountUsage` (1=Cash, 2=Bank) di Chart of Accounts; seed `1-10100` Cash dan `1-10200` Cash in Bank.
- **Sales Quotation / Sales Order / Delivery Order**: alur penjualan bertahap; DO post kurangi stok via `StockMovement` "Sales Delivery", convert ke sales invoice `SI-`. Migration `20260816224729_AddSalesFlowModule`; menu parent Sales (seq 6), sequence menu lain digeser (Inventory 8, Approvals 9, Kas & Bank 10).
- **Purchase Request**: permintaan pembelian sebelum PO; menu `Purchasing > Purchase Request`. Migration `20260816230406_AddPurchaseRequestModule`.
- **Group/Jenis Barang & Satuan**: `ItemGroup` + `Unit`. Migration `20260816231519_AddItemGroupUnitModule`; menu anak Inventory: Items 1, Stock Card 2, Stock Opname 3, Item Groups 4, Units 5.
- **Stock Transfer**: mutasi stok antar gudang; post buat 2 `StockMovement` per line ("Stock Transfer Out"/"Stock Transfer In"), nomor `ST-`. Migration `20260816232426_AddStockTransferModule`.
- **Stock Minimum**: reorder point per item (`ItemEntity.ReorderPoint`). Migration `20260816233819_AddStockMinimumModule`.
- **Receivable & Payable**: daftar detail piutang/utang per customer/supplier (AR/AP as of date, status Open/Overdue). Tanpa migrasi; menu parent Accounting seq 9.
- **Pajak & PPN**: CRUD pajak + laporan PPN (tab Tax List & PPN Report; DPP = TotalAmount − TaxAmount; PPN Keluaran vs Masukan). Tanpa migrasi; menu parent Accounting seq 10 (Taxes).
- **Aktiva Tetap**: `FixedAssetEntity` & `FixedAssetDepreciationEntity`; post jurnal Debit Depreciation Expense / Credit Accumulated Depreciation. Seed COA baru `1-10500`, `1-10600`, `5-10500`. Migration `20260817000433_AddFixedAssetModule`; menu parent Accounting seq 11.
- **Tutup Tahun**: `YearEndClosingEntity`; preview saldo P&L as-of 31-12 → post jurnal Debit pendapatan / Credit beban, balancing ke Retained Earnings (`30000000-...003`), unique index FiscalYear. Migration `20260817001308_AddYearEndClosingModule`; menu parent Accounting seq 12.
- **Jurnal Memo / Penyesuaian**: kolom `JournalEntryEntity.JournalType` (General/Adjustment/Memo); nomor `JE-`/`JA-`/`JM-`. Migration `20260817002321_AddJournalTypeToJournalEntry`; menu parent Accounting seq 13.
- **Serial Number / Batch**: `StockBatchEntity`; register batch/lot/serial, status Available/Partial/Out, aksi Consume. Migration `20260817003003_AddSerialBatchModule`; menu parent Inventory seq 8.
- **Payroll / HRM**: `EmployeeEntity`, `PayrollEntity`, `PayrollLineEntity`; seed COA `2-10300 Payroll Payable` (GUID `20000000-...020`); post jurnal Debit `5200 Salaries and Wages` / Credit `Payroll Payable`, nomor `PR-YYYYMM-XXXX`. Migration `20260817003809_AddPayrollModule`; menu parent HRM/Payroll baru seq 11.
- **Produksi / Manufacturing**: `BillOfMaterialEntity`+`BillOfMaterialLineEntity`, `ProductionOrderEntity`+`ProductionOrderLineEntity`; post cek stok komponen, buat 2 `StockMovement` ("Production Consumption"/"Production Output"), jurnal Debit Inventory barang jadi / Credit Inventory komponen; nomor `PRD-XXXXX`, BOM `BOM-XXXXX`. Migration `20260817005129_AddProductionModule`; menu parent Produksi baru seq 12. **Backlog menu Accurate selesai (41 menu terhubung).**

### 8.3 Status Hardening Transaksi
- Invoice post membuat & mem-posting jurnal otomatis untuk sales & purchase invoice.
- Payment post membuat & mem-posting jurnal otomatis untuk receipt & supplier payment, lalu update paid amount invoice.
- Detail transaksi (line, jurnal terkait, aksi status dasar) sudah ada.
- Draft bisa diedit/dihapus selama status `Draft`.
- Attachment fisik: upload, download, delete, storage configurable.
- PDF/print: print-ready page (browser Print → Save as PDF); export PDF native belum memakai DinkToPdf.
- Belum dilakukan smoke test runtime menyeluruh; validasi baru sampai `dotnet build AccuFlow.sln` berhasil.

---

## 9. Role & Permission

### 9.1 Temuan
- Permission dipakai untuk filter sidebar/menu berdasarkan `RoleMenus.CanView`.
- Backend mayoritas masih `[Authorize]`, belum enforce `CanView`/`CanAdd`/`CanEdit`/`CanDelete`/`CanPost`/`CanReverse`.
- `RoleMenuSeed` baru menyediakan admin permissions, belum dipanggil otomatis saat startup/seeding.

### 9.2 Perbaikan yang Sudah Dilakukan
- Permission filter/service untuk enforce `RoleMenus` di backend.
- Mapping action controller ke permission: `Index/Get/Datatable/Print=View`, `Create/Upload=Add`, `Edit=Edit`, `Delete=Delete`, `Post/Approve/Convert/Cancel=Post`, `Reverse=Reverse`.
- Create role return `roleId` lalu sync permission admin saat seeding.
- Sidebar: parent menu muncul otomatis jika ada child yang boleh dilihat.
- **Edit role menjadi halaman penuh** (`Role/Edit?id=`), bukan modal: form role info + access control (permissions) di satu halaman; tombol list Edit & Manage Permissions mengarah ke halaman tersebut. Tombol Add tetap modal (mengaktifkan tipe enum yang belum dipakai). Data role type dipindah ke file JS bersama `role-type-data.js`.
- **Role berbasis enum tetap**: dropdown role type diisi dari `RoleEnum` (11 tipe tetap); `RoleService.Create` menolak tipe yang sudah ada, Edit tidak mengizinkan ganti role type bila ada user aktif.

### 9.3 Super Administrator
- Role `<b>Super Administrator</b>` adalah role teknis tertinggi, berbeda dari `Administrator`.
- Dihapus dari Role Management & dropdown role (tidak bisa diedit/dihapus dari UI).
- Di list role, baris Super Administrator hanya menampilkan View Detail & Audit Trail — tombol Edit dan Manage Permissions disembunyikan karena akses penuh.
- User seed `admin` memakai role Super Administrator; user seed `administrator` memakai role Administrator.
- `SeedController` & Hangfire dashboard dibatasi ke role Super Administrator.

### 9.4 Daftar Role Final
- `Super Administrator` — role teknis tertinggi, hidden untuk seeding, Hangfire, maintenance.
- `Administrator` — admin aplikasi harian dengan full operational permission.
- `Manager` — review laporan & approval dokumen.
- `Accountant` — jurnal, posting, payment, invoice, dan laporan accounting.
- `Finance Staff` — persiapan invoice & payment operasional.
- `AR Officer` — customer invoice, receipt, AR aging.
- `AP Officer` — supplier invoice, supplier payment, AP aging.
- `Purchasing` — supplier & purchase order.
- `Sales` — customer & sales invoice.
- `Warehouse` — item, stock movement, stock opname, stock card.
- `Viewer` — read-only dashboard & laporan.

---

## 10. Rencana Lanjutan

1. Seed permission menu untuk role operasional (Manager, Accountant, dst.) agar sidebar sesuai peran.
2. Smoke test runtime end-to-end untuk seluruh transaksi inti.
3. Penyempurnaan opsional: purchase invoice & supplier payment terpisah, retur pola Accurate penuh, laporan kas/bank, payroll dengan tunjangan/potongan, BOM multi-level, integrasi stock batch ke transaksi.
4. Export PDF native via DinkToPdf bila diperlukan.

---

*Terakhir diperbarui: 2026-08-17*
*Status pembandingan: 41 menu terhubung; seluruh backlog berjumlah 0 (selesai).*