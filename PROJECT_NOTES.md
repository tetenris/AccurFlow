# Catatan Khusus Project AccuFlow

## Gambaran
- Project bernama AccuFlow, aplikasi ASP.NET Core 8 MVC untuk sistem akuntansi.
- Database memakai SQL Server via Entity Framework Core, connection default di `appsettings.json`.
- UI memakai Metronic/Bootstrap dengan Razor Views di `Views/`.
- Background job memakai Hangfire, dashboard di `/hangfire`.
- Export Excel memakai NPOI; PDF dependency DinkToPdf sudah terpasang.

## Fitur Utama
- Master: `ChartOfAccount`, `Customer`, `User`, `Role`, `Menu`, `RoleMenu`.
- Transaksi: `JournalEntry` dengan status `Draft`, `Posted`, `Reversed`.
- Laporan: `GeneralLedger`, `TrialBalance`, `FinancialStatement` untuk income statement, balance sheet, cash flow.
- Seeder otomatis berjalan saat startup: roles, users, chart of accounts, customers, menus.
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
- Ada beberapa namespace sisa template `KomatsuERP` di service/model laporan.
- `AccountService.ValidateUser` saat ini belum memverifikasi password hash, hanya cek username aktif.
- Claim login memakai `ClaimTypes.Name`, `ClaimTypes.Email`, `ClaimTypes.Role`, tapi `CurrentUserService` mencari claim literal `UserId`, `UserName`, `Email`, `RoleName`; ini berpotensi bikin current user kosong.

## Pola Pengembangan
- Ikuti pola existing: Controller tipis, logic di Service, model request/view di `Models`.
- Query EF umumnya memfilter soft delete dengan `IsDeleted`.
- Untuk fitur baru, daftarkan service di `Infrastructures/AppServiceCollection.cs`.
- Untuk entity baru, tambahkan DbSet di `Entities/Context/AppDbContext.cs` dan konfigurasi di `Entities/EntityConfigurations` bila perlu.

## Status Implementasi

### Sudah Ada
- Core app: ASP.NET Core 8 MVC, EF Core SQL Server, migration otomatis, DI, layout Metronic, cookie auth.
- Master data: `Role`, `User`, `Menu`, `RoleMenu`, `ChartOfAccount`, `Customer`.
- Accounting core: `JournalEntry`, `JournalLine`, posting, reversal, soft delete.
- Report accounting: `GeneralLedger`, `TrialBalance`, `FinancialStatement` dengan income statement, balance sheet, cash flow.
- Export Excel: chart of accounts, customer, journal entry, general ledger, trial balance, financial statements.
- Seeder: roles, users, chart of accounts, customers, menus.
- Build status: `dotnet build AccuFlow.sln` berhasil compile, masih ada warning nullability.

### Belum Ada / Belum Selesai
- Invoice/Billing belum ada entity, controller, service, model, view. Hanya ada sisa JS template `InvoiceReview` yang tidak tersambung.
- Purchase Order belum ada entity/controller/service/view; hanya disebut di TODO sample seed.
- Payment/Receipt belum ada modul.
- Inventory/Stock Management/Stock Opname/Stock Card belum ada modul.
- Supplier menu sudah disediakan di `MenuSeed.cs`, tapi tidak ada `SupplierController`, entity, service, model, atau view.
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
4. Bersihkan sisa template: hapus/abaikan JS lama seperti `invoiceriview`, `registerunit`, `budgettransferunit`, dan pastikan menu tidak mengarah ke modul kosong.
5. Tambah modul bisnis bertahap: mulai dari Supplier, lalu Invoice/Billing, Payment/Receipt, Purchase Order, Inventory, dan Aging Report.
