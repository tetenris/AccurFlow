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

### Sudah ada di kode tapi belum jadi menu/UI:
- Entity `Tax` + seed PPN 11%
- Entity warehouse (`Warehouse`) + seed gudang awal
- Entity approval request/history

---

## 3. Yang Belum Ada (Backlog)

Dikerjakan berurutan dari 1 ke atas (pelan-pelan sesuai urutan).

- **[1] Tutup Tahun (Year-End Closing)** — proses akhir tahun akuntansi.
- **[2] Jurnal Penyesuaian / Jurnal Memo** — jurnal khusus non-operative.
- **[3] Serial Number / Batch** — pelacakan stok per unit.
- **[4] Payroll / HRM** — penggajian.
- **[5] Produksi / Manufacturing** — BOM & job order.

---

## 4. Catatan Kondisi Saat Ini
- Semua role (kecuali Super Administrator & Administrator) belum punya seed permission menu (`SeedRoleMenus` hanya mengisi 2 role sistem). Backlog permission per role perlu dibuat agar menu tampil sesuai peran.
- Sebagian modul bisnis (Invoice, Payment, PO) menggabungkan jenis sales & purchasing dalam satu tempat; jika ingin persis Accurate sebaiknya dipisah ke menu Penjualan dan Pembelian.

---

*Catatan dibuat: 2026-08-16 (diperbarui 2026-08-17)*
*Status pembandingan: menu saat ini 36 item terhubung; total backlog berjumlah 5 item berurutan.*