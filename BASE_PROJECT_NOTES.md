# Catatan Base Project

## Tujuan
- Project ini dibuat sebagai base project yang diambil dari aplikasi yang sudah ada.
- Scope diperkecil agar hanya berisi fondasi aplikasi umum, bukan modul bisnis penuh.
- Kondisi saat ini dianggap sudah sesuai untuk kebutuhan base project.

## Scope Yang Dipertahankan
- Login dan autentikasi user.
- Reset password / pengelolaan password dasar.
- Dashboard awal setelah login.
- Master User.
- Master Role.
- Permission/menu dasar yang diperlukan untuk user dan role.

## Scope Yang Dikeluarkan
- Modul accounting, transaksi, invoice, payment, purchase order, inventory, report bisnis, dan modul operasional lain tidak menjadi bagian base project.
- Entity, menu, seeder, controller, view, dan migration yang tidak terkait login, dashboard, user, role, atau permission dasar sebaiknya tidak dipertahankan.
- Base project ini harus tetap ringan agar bisa dipakai ulang untuk aplikasi lain.

## Catatan Implementasi Saat Ini
- Perubahan yang ada di working tree saat ini adalah hasil pemangkasan dari aplikasi existing menuju base project.
- Migration baru dibuat untuk menjaga struktur database hanya pada tabel dasar identity/profile/role/menu yang dibutuhkan.
- `Views/Account/AccessDenied.cshtml` ditambahkan sebagai halaman akses ditolak untuk permission/auth.
- Seeder difokuskan untuk data awal user, role, menu, dan permission dasar.

## Prinsip Lanjutan
- Jangan menambahkan kembali modul bisnis sebelum ada kebutuhan aplikasi baru.
- Jika base project dipakai untuk project lain, mulai dari fitur login, reset password, dashboard, user, dan role ini.
- Perubahan berikutnya sebaiknya hanya hardening fondasi: security, validation, UX login, dan kerapihan seed/migration.

## Cara Memakai Branch Ini Untuk Project Baru
- Buat branch baru dari branch base project ini agar baseline tetap bersih.
- Ganti nama project, namespace, assembly name, dan judul aplikasi sesuai project baru.
- Ubah connection string di `appsettings.json` ke database project baru.
- Buat database baru, lalu jalankan migration dari kondisi base project.
- Sesuaikan seeder user, role, menu, dan permission awal sesuai kebutuhan project baru.
- Jalankan aplikasi dan pastikan login, reset password, dashboard, user, dan role berjalan sebelum menambah modul lain.
- Setelah fondasi stabil, mulai tambahkan modul bisnis project baru secara bertahap.

## Checklist Awal Project Baru
- Rename solution/project bila diperlukan.
- Update namespace lama agar konsisten dengan nama project baru.
- Update nama database dan konfigurasi environment.
- Reset atau sesuaikan data seed default.
- Jalankan `dotnet restore`, `dotnet build`, dan update database.
- Commit baseline project baru sebelum mulai implementasi fitur bisnis.

## Status
- Status saat ini: benar untuk kebutuhan base project.
- Langkah aman berikutnya: commit perubahan ini sebagai baseline base project.
