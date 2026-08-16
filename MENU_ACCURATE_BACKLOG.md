# Catatan Menu AccuFlow vs Accurate + Backlog

Dokumen ini mencatat perbandingan menu aplikasi AccuFlow dengan software akuntansi Accurate (versi desktop/online), daftar yang sudah ada, dan yang belum ada sebagai backlog.

Dibuat di branch: `feature/user-management` (aplikasi lengkap).

## Tujuan
- Menyamakan scope modul AccuFlow dengan kebutuhan standar software akuntansi gaya Accurate.
- Backlog digunakan sebagai acuan prioritas pengembangan berikutnya.

---

## 1. Struktur Menu Accurate (Referensi Nyata)

Modul standar Accurate kurang lebih sebagai berikut:

### Dashboard
- Ringkasan performa, grafik penjualan/pembelian.

### Penjualan (Sales)
- Sales Quotation
- Sales Order
- Pengiriman Barang (Delivery Order / Surat Jalan)
- Faktur Penjualan (Sales Invoice)
- Penerimaan Penjualan (Sales Receipt)
- Retur Penjualan (Sales Return)
- Laporan penjualan (per pelanggan, per barang, komisi, dsb.)

### Pembelian (Purchasing)
- Purchase Request / Permintaan Pembelian
- Purchase Order
- Penerimaan Barang (Goods Received / GRN)
- Faktur Pembelian (Purchase Invoice)
- Pengeluaran Pembelian (Supplier Payment)
- Retur Pembelian (Purchase Return)
- Laporan pembelian

### Persediaan (Inventory)
- Data Barang/Jasa (dan jenis/group barang, satuan)
- Harga (level harga, harga jual/beli)
- Kartu Stok (Stock Card)
- Stock Opname
- Stock Transfer / Mutasi antar gudang
- Stok Minimum / Reorder Point
- Serial Number / Batch (opsional modul akurat tertentu)

### Piutang & Utang (AR / AP)
- Daftar piutang per pelanggan
- Daftar utang per pemasok
- Aging Report piutang & utang

### Kas & Bank
- Akun Kas & Bank
- Transfer Kas / Bank (antar akun)
- Rekonsiliasi Bank
- Mutasi kas & bank

### Pajak
- Tarif PPN / PPh
- Pelaporan PPN (efaktur / summary)

### Aktiva Tetap (Fixed Assets)
- Daftar aktiva, penyusutan (depresiasi)

### Akuntansi
- Bagan Akun (Chart of Accounts)
- Jurnal Umum / Jurnal Memo
- Jurnal Penyesuaian
- Neraca Saldo (Trial Balance) & Laporan keuangan (Neraca, Laba Rugi, Arus Kas)
- Tutup Tahun (Year-End Closing)

### HRM / Payroll (opsional)
- Data karyawan, penggajian

### Produksi (Manufacturing) (opsional/versi khusus)
- Bill of Material, job order

### Utility / Setting
- User & role management, backup, konfigurasi

---

## 2. Yang Sudah Ada di AccuFlow (branch feature/user-management)

### Menu + Controller + View sudah tersambung:
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
| 15 | Purchasing > Purchase Orders | PurchaseOrder | draft/approve/convert/print |
| 16 | Inventory > Items | Inventory | create item |
| 17 | Inventory > Stock Card | Inventory | stock movement history |
| 18 | Approvals | Approval | submit/approve/reject |
| 19 | Document Attachment (via modul) | DocumentAttachment | upload/download/delete |
| 20 | Inventory > Stock Opname | StockOpname | CRUD + post (input hasil, selisih, stock adjustment) |

### Sudah ada di kode tapi belum jadi menu/UI:
- Entity `Tax` + seed PPN 11%
- Entity warehouse (`Warehouse`) + seed gudang awal
- Entity approval request/history

---

## 3. Yang Belum Ada (Backlog)

Dikerjakan berurutan dari 1 ke atas (pelan-pelan sesuai urutan).

- **[1] Modul Kas & Bank** — akun kas/bank, transfer kas/bank antar akun, rekonsiliasi bank, mutasi kas & bank. (kritikal, belum ada sama sekali)
- **[2] Penerimaan Barang (GRN)** — menerima barang dari PO, otomatis update stok & jurnal persediaan.
- **[3] Retur Penjualan & Pembelian** — retur mengurangi piutang/utang dan stok.
- **[4] Sales Quotation / Sales Order / Delivery Order** — alur penjualan bertahap (quote → order → kirim → invoice).
- **[5] Purchase Request** — permintaan pembelian sebelum dibuatkan PO.
- **[6] Group/ Jenis Barang & Satuan** — pengelompokan item agar laporan lebih detail.
- **[7] Stock Transfer / Mutasi antar gudang** — pindah stok antar warehouse.
- **[8] Stok Minimum / Reorder Point** — notifikasi stok rendah.
- **[9] Daftar Piutang & Daftar Utang per customer/supplier** — selain aging, tampilkan list piutang/utang detail.
- **[10] CRUD Pajak (tarif & pemakaian di invoice) + pelaporan PPN sederhana** — entity & seed PPN 11% sudah ada, tinggal UI.
- **[11] Aktiva Tetap (Fixed Assets)** — daftar aktiva + penyusutan.
- **[12] Tutup Tahun (Year-End Closing)** — proses akhir tahun akuntansi.
- **[13] Jurnal Penyesuaian / Jurnal Memo** — jurnal khusus non-operative.
- **[14] Serial Number / Batch** — pelacakan stok per unit.
- **[15] Payroll / HRM** — penggajian.
- **[16] Produksi / Manufacturing** — BOM & job order.

---

## 4. Catatan Kondisi Saat Ini
- Semua role (kecuali Super Administrator & Administrator) belum punya seed permission menu (`SeedRoleMenus` hanya mengisi 2 role sistem). Backlog permission per role perlu dibuat agar menu tampil sesuai peran.
- Sebagian modul bisnis (Invoice, Payment, PO) menggabungkan jenis sales & purchasing dalam satu tempat; jika ingin persis Accurate sebaiknya dipisah ke menu Penjualan dan Pembelian.

---

*Catatan dibuat: 2026-08-16 (diperbarui 2026-08-17)*
*Status pembandingan: menu saat ini 20 item terhubung; total backlog berjumlah 16 item berurutan.*