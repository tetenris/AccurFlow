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
