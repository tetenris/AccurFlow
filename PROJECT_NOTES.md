# Catatan Khusus Project AccuFlow

## Gambaran
- Project bernama AccuFlow, aplikasi ASP.NET Core 8 MVC untuk sistem akuntansi.
- Database memakai SQL Server via Entity Framework Core, connection default di `appsettings.json`.
- UI memakai Metronic/Bootstrap dengan Razor Views di `Views/`.
- Background job memakai Hangfire, dashboard di `/hangfire`.
- Export Excel memakai NPOI; PDF dependency DinkToPdf sudah terpasang.

## Fitur Utama
- Master: `ChartOfAccount`, `Customer`, `Supplier`, `User`, `Role`, `Menu`, `RoleMenu`.
- Transaksi: `JournalEntry` dengan status `Draft`, `Posted`, `Reversed`.
- Kas & Bank: `CashBankTransfer` (transfer kas/bank, posting auto jurnal), `BankReconciliation` (rekonsiliasi bank dengan load statement dari journal).
- Laporan: `GeneralLedger`, `TrialBalance`, `FinancialStatement` untuk income statement, balance sheet, cash flow.
- Seeder otomatis berjalan saat startup: roles, users, chart of accounts, customers, suppliers, menus.
- Auth memakai cookie login di `/Account/Login`.

## Arsitektur
- Controller MVC ada di `Controllers/`.
- Business logic ada di `Services/`.
- Entity dan konfigurasi EF ada di `Entities/Entity` dan `Entities/EntityConfigurations`.
- Request/view model ada di `Models/`.
- Dependency injection didaftarkan di `Infrastructures/AppServiceCollection.cs`.
- Startup pipeline ada di `Program.cs`.

## Catatan Penting
- `Program.cs` menjalankan `Database.MigrateAsync()` otomatis saat aplikasi start.
- User seed default: `admin / Admin123!` dan `accountant / Accountant123!`.
- Auth/login sudah memverifikasi password hash dengan BCrypt dan claim login sudah selaras dengan `CurrentUserService`.
- Namespace sisa template `KomatsuERP` sudah dirapikan ke `AccuFlow`.

## Pola Pengembangan
- Ikuti pola existing: Controller tipis, logic di Service, model request/view di `Models`.
- Query EF umumnya memfilter soft delete dengan `IsDeleted`.
- Untuk fitur baru, daftarkan service di `Infrastructures/AppServiceCollection.cs`.
- Untuk entity baru, tambahkan DbSet di `Entities/Context/AppDbContext.cs` dan konfigurasi di `Entities/EntityConfigurations` bila perlu.

## Status Implementasi

### Sudah Ada
- Core app: ASP.NET Core 8 MVC, EF Core SQL Server, migration otomatis, DI, layout Metronic, cookie auth.
- Master data: `Role`, `User`, `Menu`, `RoleMenu`, `ChartOfAccount`, `Customer`, `Supplier`.
- Accounting core: `JournalEntry`, `JournalLine`, posting, reversal, soft delete.
- Report accounting: `GeneralLedger`, `TrialBalance`, `FinancialStatement` dengan income statement, balance sheet, cash flow.
- Export Excel: chart of accounts, customer, supplier, journal entry, general ledger, trial balance, financial statements.
- Alur penjualan: `SalesQuotation` (draft/approve), `SalesOrder` (draft/approve, dari quote), `DeliveryOrder` (draft/post, kurangi stok, convert ke invoice).
- Alur pembelian: `PurchaseRequest` (draft/approve, convert ke PO sesuai supplier & tanggal).
- Master inventory: `ItemGroup` (group/jenis barang) & `Unit` (satuan) dengan CRUD + seed; `Item` ber-relasi ke group.
- Seeder: roles, users, chart of accounts, customers, suppliers, menus.
- Build status: `dotnet build AccuFlow.sln` berhasil compile, masih ada warning nullability.

### Belum Ada / Belum Selesai
- Invoice/Billing belum ada entity, controller, service, model, view. Hanya ada sisa JS template `InvoiceReview` yang tidak tersambung.
- Purchase Order belum ada entity/controller/service/view; hanya disebut di TODO sample seed.
- Payment/Receipt belum ada modul.
- Inventory/Stock Management/Stock Opname/Stock Card belum ada modul.
- Aging Report belum ada.
- Approval workflow belum ada backend; hanya ada partial view `_HistoryApprovalModal.cshtml` dan sisa JS invoice approval.
- PDF generation belum dipakai walaupun package `DinkToPdf` sudah terpasang.

### Masalah Teknis Penting
- Auth/login sudah diperbaiki: `UseAuthentication`, validasi BCrypt, claim user lengkap, dan parsing `Guid` di `CurrentUserService`.
- Namespace sisa `KomatsuERP` sudah dirapikan ke `AccuFlow`.
- `SeedController` sudah diproteksi dengan role `Administrator`.
- Hangfire dashboard sudah memakai auth role berbasis cookie login; role wajib diatur lewat `Hangfire:Dashboard:RequiredRole`.

### Prioritas Lanjutan
1. Bereskan auth dulu: `UseAuthentication`, validasi BCrypt, claim user lengkap, proteksi seed/hangfire.
2. Rapikan namespace `KomatsuERP` ke `AccuFlow`.
3. Pilih scope fitur berikutnya: Invoice, Payment/Receipt, atau Supplier.
4. Bersihkan sisa template JS yang tidak terpakai agar tidak membingungkan.
5. Tambahkan modul transaksi non-jurnal secara bertahap, lalu integrasikan otomatis ke journal entry.

## Urutan Perbaikan Fondasi
1. Selesai - Perbaiki auth/login: tambah `UseAuthentication`, validasi BCrypt, claim user lengkap, dan pastikan `CurrentUserService` terbaca benar.
2. Selesai - Amankan akses berisiko: aktifkan authorize di `SeedController`, batasi ke Administrator, dan ganti credential Hangfire hardcoded dengan role auth dari konfigurasi.
3. Selesai - Rapikan namespace: ganti sisa `KomatsuERP` ke `AccuFlow`.
4. Selesai - Bersihkan sisa template: hapus JS lama `invoiceriview`, `registerunit`, `budgettransferunit`, dan pastikan menu tidak mengarah ke modul kosong.
5. Berjalan - Modul bisnis tahap awal sudah dibuat tanpa commit: Invoice/Billing, Payment/Receipt, Purchase Order, Inventory, Aging Report, Approval Workflow, dan Document Attachment. Masih perlu hardening detail transaksi, posting jurnal otomatis lengkap, PDF export, upload file fisik, dan testing UI end-to-end.

## Status Roadmap Bisnis
- Invoice/Billing: entity, migration, service, controller, menu, view list, JS datatable, create draft, post, cancel.
- Payment/Receipt: entity, migration, service, controller, menu, view list, JS datatable, allocation invoice, post payment.
- AR/AP Aging: report dari invoice outstanding dengan bucket current, 1-30, 31-60, 61-90, dan >90 hari.
- Purchase Order: entity, migration, service, controller, menu, view list, approve, convert to purchase invoice.
- Inventory: item, warehouse, stock movement, stock opname entity, item list, stock card list, seed warehouse awal.
- Approval Workflow: approval request/history entity, service, controller, view list, approve/reject dasar.
- Document Attachment: entity dan service datatable dasar; upload/download file fisik belum diimplementasikan.
- Tax Management: entity dan seed PPN 11%; UI CRUD tax belum dibuat.
- Kas & Bank: modul lengkap dengan menu `Cash & Bank Accounts` (daftar akun + saldo), `Cash Bank Transfers` (create/edit/delete draft, post auto-jurnal DR akun tujuan / CR akun asal), dan `Bank Reconciliation` (load statement dari journal, flag Cleared/Float, validasi balance saat post). Kolom `AccountUsage` (1=Cash, 2=Bank) ditambahkan di Chart of Accounts via migration `20260816043832_AddCashBankModule`; seed menandai `1-10100` Cash dan `1-10200` Cash in Bank.
- Sales Quotation / Sales Order / Delivery Order: alur penjualan bertahap. Menu `Penjualan > Sales Quotation` (CRUD draft, approve → jadi sumber SO), `Penjualan > Sales Order` (CRUD draft, approve, line otomatis dari quotation ter-approved), `Penjualan > Delivery Order` (CRUD draft, post kurangi stok via `StockMovement` "Sales Delivery", convert ke sales invoice `SI-`). Migrasi `20260816224729_AddSalesFlowModule`; menu parent Sales (seq 6), sequence menu lain digeser (Inventory 8, Approvals 9, Kas & Bank 10).
- Purchase Request: permintaan pembelian sebelum PO. Menu `Purchasing > Purchase Request` (CRUD draft, approve, convert ke Purchase Order Draft dengan pilih supplier + tanggal; line otomatis tersalin, nomor `PR-` & `PO-`). Migrasi `20260816230406_AddPurchaseRequestModule`; menu parent Purchasing (seq 7), anak diurutkan Purchase Request (1), Purchase Orders (2), Goods Received (3).
- Group/Jenis Barang & Satuan: menu `Inventory > Item Groups` & `Inventory > Units` (CRUD + seed group General/Raw Material/Finished Goods/Spare Part; unit PCS/BOX/SET/KG/L). `ItemEntity` mendapat `ItemGroupId` (FK) dan kolom group tampil di daftar Inventory. Migrasi `20260816231519_AddItemGroupUnitModule`; menu anak Inventory berurutan Items (1), Stock Card (2), Stock Opname (3), Item Groups (4), Units (5).
- Stock Transfer: mutasi stok antar gudang. Menu `Inventory > Stock Transfer` (draft/edit/delete, post cek stok tersedia lalu buat 2 `StockMovement` per line: `Stock Transfer Out` dari gudang asal & `Stock Transfer In` ke gudang tujuan, sumber `StockTransfer`, nomor `ST-`). Validasi From ≠ To warehouse. Migrasi `20260816232426_AddStockTransferModule`; menu anak Inventory kini berurutan Items (1), Stock Card (2), Stock Opname (3), Item Groups (4), Units (5), Stock Transfer (6), Stock Minimum (7).
- Stock Minimum: reorder point per item (`ItemEntity.ReorderPoint`). Menu `Inventory > Stock Minimum` menampilkan daftar item + stok saat ini (dari `StockMovement`) dengan badge "Below Minimum" bila stok < reorder point; kolom reorder point bisa di-update langsung via modal (endpoint `Inventory/UpdateReorderPoint`). Migrasi `20260816233819_AddStockMinimumModule`.

## Status Hardening Transaksi
- Invoice post sekarang membuat dan mem-posting jurnal otomatis untuk sales invoice dan purchase invoice.
- Payment post sekarang membuat dan mem-posting jurnal otomatis untuk receipt dan supplier payment, lalu update paid amount invoice.
- Invoice list sudah punya modal create sederhana untuk satu baris transaksi.
- Payment list sudah punya modal create sederhana dengan alokasi satu invoice.
- Purchase Order list sudah punya modal create sederhana untuk satu baris PO.
- Belum dilakukan smoke test runtime; validasi baru sampai `dotnet build AccuFlow.sln` berhasil.

## Rencana Lanjutan Hardening
1. Selesai - Detail transaksi: Invoice detail, Payment detail, dan Purchase Order detail dengan line, jurnal terkait, dan tombol aksi status dasar.
2. Selesai - Edit/delete draft: Invoice, Payment, dan Purchase Order bisa diedit/dihapus selama status masih `Draft`.
3. Selesai - Attachment fisik: upload, download, delete file, dan storage configurable untuk Invoice, Payment, dan Purchase Order.
4. Selesai - PDF/print: print-ready page untuk invoice, payment receipt, dan purchase order. Export PDF native belum memakai DinkToPdf; browser print dapat Save as PDF.
5. Smoke test runtime setelah transaksi inti dan dokumen siap.

## Temuan Role dan Permission
- Permission saat ini sudah dipakai untuk filter sidebar/menu berdasarkan `RoleMenus.CanView`.
- Backend mayoritas masih hanya `[Authorize]`, belum enforce `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanPost`, atau `CanReverse`.
- `RoleController.Create` perlu return `roleId` agar permission role baru bisa langsung tersimpan dari UI.
- `RoleMenuSeed` baru menyediakan admin permissions, tapi belum dipanggil otomatis saat startup/seeding.
- Parent menu hanya muncul jika parent punya `CanView`; perlu dibuat muncul otomatis jika ada child yang punya `CanView`.
- Action permission belum lengkap/sinkron: ada action seperti `cancel`, `approve`, `convert`, tapi `RoleMenuEntity` baru punya view/add/edit/delete/post/reverse.
- `RoleEntity.Permissions` JSON belum dipakai untuk enforcement sehingga sementara redundant.

## Rencana Perbaikan Role dan Permission
1. Selesai - Tambah permission filter/service untuk enforce `RoleMenus` di backend.
2. Selesai - Mapping action controller ke permission: `Index/Get/Datatable/Print=View`, `Create/Upload=Add`, `Edit=Edit`, `Delete=Delete`, `Post/Approve/Convert/Cancel=Post`, `Reverse=Reverse`.
3. Selesai - Fix create role return `roleId`, lalu sync permission admin saat seeding.
4. Selesai - Perbaiki sidebar agar parent menu tetap muncul jika ada child yang boleh dilihat.
5. Sembunyikan tombol UI berdasarkan permission setelah backend enforcement siap.

## Super Administrator
- Role `Super Administrator` ditambahkan sebagai role teknis tertinggi dan berbeda dari `Administrator`.
- `Super Administrator` disembunyikan dari Role Management dan dropdown role agar tidak bisa diedit/dihapus dari UI operasional.
- User seed `admin` memakai role `Super Administrator`; user seed `administrator` memakai role `Administrator`.
- `SeedController` dan Hangfire dashboard dibatasi ke role `Super Administrator`.
- `Administrator` tetap mendapat full operational permission melalui sync `RoleMenus`, tapi tidak bypass permission filter seperti `Super Administrator`.

## Daftar Role Final
- `Super Administrator`: role teknis tertinggi, hidden dari menu role/user operasional, untuk seeding, Hangfire, dan maintenance.
- `Administrator`: admin aplikasi harian dengan full operational permission.
- `Manager`: review laporan dan approval dokumen.
- `Accountant`: jurnal, posting, payment, invoice, dan laporan accounting.
- `Finance Staff`: persiapan invoice dan payment operasional.
- `AR Officer`: customer invoice, receipt, dan AR aging.
- `AP Officer`: supplier invoice, supplier payment, dan AP aging.
- `Purchasing`: supplier dan purchase order.
- `Sales`: customer dan sales invoice.
- `Warehouse`: item, stock movement, stock opname, dan stock card.
- `Viewer`: read-only dashboard dan laporan.
