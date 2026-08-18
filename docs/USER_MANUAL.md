# User Manual — AccuFlow Accounting System

Panduan penggunaan aplikasi untuk user. Manual ini menjelaskan menu, fungsi tiap halaman, aksi (tombol), serta tabel dan kolom yang tampil di setiap modul.

---

## 1. Login

Buka aplikasi lalu login dengan kredensial di bawah ini.

### User bawaan (default seed)

| Username      | Password           | Role / Hak Akses      |
|---------------|--------------------|-----------------------|
| `admin`       | `Admin123!`        | Super Administrator   |
| `administrator` | `Admin123!`      | Administrator         |
| `accountant`  | `Accountant123!`   | Accountant            |

> **Catatan:** Setelah login pertama, jika password berumur lebih dari 1 bulan, sistem akan mewajibkan Anda mengganti password sebelum bisa melanjutkan.

### Halaman yang tersedia di menu Akun
- **Login** — Masukkan Username atau Email + Password, lalu klik **Sign In**.
- **Lupa Password** — Masukkan email; instruksi reset akan dikirim jika layanan email aktif.
- **Change Password** — Ganti password sendiri (wajib: min 8 karakter, kombinasi huruf besar, huruf kecil, angka, dan simbol).
- **Logout** — Keluar dari aplikasi (ikon di header).

---

## 2. Dashboard (Home)

Halaman utama setelah login — "Finance command center".

- Sapaan **"Selamat datang, [Nama]"** + role user yang login.
- **Tombol hero:** `New Journal` (→ Journal Entry), `Open Invoices` (→ Invoices), `Ledger` (→ General Ledger).
- **Quick Actions:** 6 pintasan menu (Chart of Accounts, Invoices, Payments, Purchase Orders, User Access, Trial Balance).
- **Journal posting health:** ring persentase jurnal ter-posting vs draft.
- **Kartu metrik (Rupiah):** Outstanding receivables, Outstanding payables, Overdue invoices, Cash movement this month.
- **Recent Activity:** 6 aktivitas terakhir (jurnal & invoice).

---

## 3. User Management

Menu induk berisi 2 halaman: **Users** dan **Roles**. Menu sidebar tampil dinamis sesuai permission role masing-masing user.

### 3.1 Users
**Fungsi:** Mengelola akun pengguna (tambah, edit, hapus, atur role & status, buka kunci, reset password).

**Aksi:**
- Toolbar: `Add New User`
- Per baris: `View Detail`, `Edit`, `Delete` (hanya user non-aktif), `Unlock / Reset Password` (muncul jika user terkunci — password direset ke `Qwerty@123` dan wajib diganti saat login berikutnya), `Audit Trail`
- Filter: Role, Status (Active/Inactive), `Apply Filter`, + pencarian

**Tabel `user_datatable`:** No, Username, Email, Full Name, Role, Status, Action.

### 3.2 Roles
**Fungsi:** Mengelola role beserta hak akses menu (permission).

**Aksi:**
- Toolbar: `Add New Role`
- Per baris: `View Detail`, `Edit Role`, `Manage Permissions` (ikon kunci), `Audit Trail`
- Filter: Status + `Apply Filter`

**Tabel `role_datatable`:** No, Role Name, Description, Status, Action.

**Assign Permission:** checkbox per menu + aksi (`View`, `Add`, `Edit`, `Delete`, `Post`, `Reverse`), plus **Select All** untuk full access.

**Role Type bawaan:** Super Administrator, Administrator, Manager, Accountant, Finance Staff, AR Officer, AP Officer, Purchasing, Sales, Warehouse, Viewer.

---

## 4. Master

Menu induk berisi: **Chart of Accounts**, **Customers**, **Suppliers**.

### 4.1 Chart of Accounts
**Fungsi:** Master daftar akun (COA) hierarkis (parent–child), menentukan normal balance, saldo awal, mata uang, status header akun.

**Aksi:** `Add Account`, `Export Excel`; per baris: `View Detail`, `Edit`, `Delete`, `Toggle Status`; filter: Account Type, Status.

**Tabel:** No, Account Code, Account Name, Type, Parent, Status, Action.

### 4.2 Customers
**Fungsi:** Mengelola data pelanggan (identitas, kontak, alamat, kredit).

**Aksi:** `Add Customer`, `Export Excel`; per baris: `View Detail`, `Edit`, `Delete`, `Toggle Status`; filter: Customer Type, Status.

**Tabel `customer_datatable`:** No, Customer Code, Customer Name, Type, Contact Person, Phone, Email, Status, Action.

**Form (5 tab):** Basic Info, Contact, Address, Financial (Credit Limit, Payment Terms, Tax ID), Additional.

### 4.3 Suppliers
**Fungsi:** Mengelola data pemasok (identik dengan Customers).

**Aksi:** `Add Supplier`, `Export Excel`; per baris: `View Detail`, `Edit`, `Delete`, `Toggle Status`; filter: Supplier Type, Status.

**Tabel `supplier_datatable`:** No, Supplier Code, Supplier Name, Type, Contact Person, Phone, Email, Status, Action.

---

## 5. Accounting

Menu induk dengan 13 halaman laporan & transaksi keuangan.

### 5.1 Journal Entry
**Fungsi:** Pencatatan jurnal umum manual multi-baris (debit/kredit), status Draft → Posted, bisa di-reverse.

**Aksi:** `Add Journal`, `Export Excel`; per baris: `View Detail`, `Edit` (draft); di detail: `Post Journal`, `Reverse Journal`; filter: Date, Status, Account.

**Tabel:** No, Journal Number, Date, Description, Total Amount, Status, Action.

### 5.2 General Ledger
**Fungsi:** Ringkasan mutasi per akun per periode; klik `View Ledger` untuk detail transaksi.

**Aksi:** `Export Summary`; per baris: `View Ledger`; filter: Date From/To, Account Type.

**Tabel ringkasan:** Account Code, Account Name, Type, Total Debit, Total Credit, Balance, Transactions, Actions.
**Tabel Account Ledger:** Date, Journal Number, Description, Debit, Credit, Balance (dengan Opening & Closing Balance).

### 5.3 Trial Balance
**Fungsi:** Neraca saldo per tanggal, dikelompokkan per tipe akun, dengan subtotal & total (seimbang/tidak).

**Aksi:** `Export Excel`; filter: As of Date, Account Type, checkbox Show Zero Balance.

**Tabel:** Account Code, Account Name, Debit, Credit (+ baris subtotal & TOTAL).

### 5.4 Financial Statements
**Fungsi:** Laporan keuangan dalam 3 tab — Income Statement, Balance Sheet, Cash Flow.

**Aksi:** `Export Excel` (tab aktif); filter per tab (periode/tanggal + Show Zero Balance).

**Tabel:** Account Code, Account Name, Amount.

### 5.5 Invoices
**Fungsi:** Kelola invoice Penjualan (Sales) & Pembelian (Purchase), status Draft → Posted → Partially Paid → Paid / Cancelled; menampilkan sisa tagihan (Outstanding).

**Aksi:** `Add Invoice`; per baris: `Detail`, `Edit`, `Delete` (draft), `Post` (draft); di detail: `Print / PDF`, `Upload / Download / Delete` lampiran; filter: Invoice Type, Status.

**Tabel:** No, Invoice No, Type, Date, Due Date, Partner, Status, Total, Outstanding, Action.

### 5.6 Payments & Receipts
**Fungsi:** Mencatat pembayaran (Payment) & penerimaan (Receipt), dialokasikan ke invoice, metode Bank Transfer/Cash/Giro.

**Aksi:** `Add Payment`; per baris: `Detail`, `Edit`, `Delete` (draft), `Post` (draft); di detail: `Print / PDF`, lampiran; filter: Type, Status.

**Tabel:** No, Payment No, Type, Date, Partner, Method, Status, Total, Action.

### 5.7 Aging Report
**Fungsi:** Laporan umur piutang (AR) / utang (AP) berdasarkan rentang hari jatuh tempo.

**Aksi:** filter Aging Type (AR/AP Aging) + As Of Date → `Generate`.

**Tabel:** Partner Code, Partner Name, Current, 1-30, 31-60, 61-90, >90, Total.

### 5.8 Returns
**Fungsi:** Retur penjualan & pembelian terhadap invoice; saat Post stok berkurang & jurnal dibuat.

**Aksi:** `Add Return`; per baris: `Detail`, `Edit`, `Delete` (draft), `Post` (draft); filter: Return Type, Status.

**Tabel:** No, Return No, Type, Date, Invoice, Partner, Status, Lines, Total, Action.
**Tabel item detail:** Item, Description, Invoice Qty, Remaining, Return Qty, Unit Price, Tax, Line Total.

### 5.9 Receivable & Payable
**Fungsi:** Laporan rincian piutang (AR) / utang (AP) per invoice per partner (terbayar, sisa, status Overdue/Open).

**Aksi:** filter Report Type (AR/AP) + As Of Date → `Generate`.

**Tabel:** Partner Code, Partner Name, Invoice No, Invoice Date, Due Date, Total, Paid, Outstanding, Status.

### 5.10 Taxes
**Fungsi:** Master tarif pajak (VAT, Withholding, Other) + laporan PPN periodik.

**Aksi:** tab Tax List: `Add Tax`, `Edit`, `Delete`; tab PPN Report: filter From/To Date → `Generate` (kartu PPN Keluaran, PPN Masukan, PPN Bersih).

**Tabel Tax List:** No, Tax Code, Tax Name, Rate (%), Type, Status, Action.
**Tabel PPN Report:** Invoice No, Type, Partner, Date, DPP, PPN, Total.

### 5.11 Fixed Assets
**Fungsi:** Master aset tetap (perolehan, nilai sisa, umur manfaat, akumulasi penyusutan, Net Book Value).

**Aksi:** `Add Asset`; per baris: `Detail`, `Edit`, `Delete`, `Depreciate` (status Active → modal Run Depreciation dengan **Period Date** → `Run`, membuat jurnal otomatis).

**Tabel:** No, Asset Code, Asset Name, Category, Purchase Date, Cost, Accum. Depr., Net Book Value, Status, Action.

### 5.12 Year-End Closing
**Fungsi:** Penutupan tahun fiskal — menutup akun pendapatan & beban ke Retained Earnings via jurnal otomatis.

**Aksi:** pilih **Fiscal Year** → `Preview` → `Close Fiscal Year` (dengan konfirmasi). Menampilkan kartu Total Revenue/Expense/Net Income + Riwayat Penutupan.

**Tabel preview akun:** Account Code, Account Name, Balance.
**Tabel Riwayat Penutupan:** Fiscal Year, Closing Date, Journal No.

### 5.13 Jurnal Memo / Penyesuaian
**Fungsi:** Jurnal memo/penyesuaian manual (tipe Memo atau Adjustment), Draft → Posted, bisa di-reverse.

**Aksi:** `Add Journal`; per baris: `View Detail`, `Edit` (draft); di detail: `Post Journal`, `Reverse Journal`; filter: Date, Type, Status.

**Tabel:** No, Journal Number, Date, Description, Total Amount, Status, Action.

---

## 6. Sales

Menu induk berisi 3 halaman alur penjualan: Quotation → Order → Delivery.

### 6.1 Sales Quotation
**Fungsi:** Membuat & mengelola penawaran harga ke customer. Status: Draft → Approved.

**Aksi:** `Add Quotation`, `Add Line`, `Save`; per baris: `Detail`, `Edit`, `Approve`, `Delete` (Approve/Delete hanya saat Draft); filter status.

**Tabel `quote_datatable`:** No, Quote No, Date, Valid Until, Customer, Status, Lines, Total, Action.
**Tabel item detail:** Item, Description, Qty, Unit Price, Discount, Tax, Line Total.

### 6.2 Sales Order
**Fungsi:** Pesanan penjualan; bisa diturunkan dari Quotation **Approved** (dropdown "From Quotation"). Status: Draft → Approved.

**Aksi:** `Add Sales Order`, `Add Line`, `Save`; per baris: `Detail`, `Edit`, `Approve`, `Delete` (saat Draft); filter status.

**Tabel `order_datatable`:** No, Order No, Order Date, Expected, Customer, Quote, Status, Lines, Total, Action.

### 6.3 Delivery Order
**Fungsi:** Pengiriman barang berdasarkan Sales Order **Approved**. Status: Draft → Posted → Invoiced.

**Aksi:** `Add Delivery Order`, `Save`; per baris: `Detail`, `Edit`, `Post` (Draft; mengurangi stok + jurnal inventory), `Delete` (Draft), `To Invoice` (saat Posted → konversi ke invoice penjualan); filter status.

**Tabel `delivery_datatable`:** No, Delivery No, Date, Sales Order, Customer, Status, Lines, Total, Action.
**Tabel item modal:** Ordered Qty, Remaining, Deliver Qty, Unit Price, Line Total.

---

## 7. Purchasing

Menu induk berisi 3 halaman alur pembelian: Purchase Request → Purchase Order → Goods Received.

### 7.1 Purchase Request
**Fungsi:** Permintaan pembelian internal. Status: Draft → Approved → Converted.

**Aksi:** `Add Purchase Request`, `Add Line`, `Save`; per baris: `Detail`, `Edit`, `Approve`, `Delete` (Draft), `Convert` (saat Approved → pilih Supplier + Expected Date); filter status.

**Tabel `pr_datatable`:** No, PR No, Request Date, Required, Requested By, Department, Status, Lines, Total, PO, Action.

### 7.2 Purchase Orders
**Fungsi:** Pesanan pembelian ke supplier. Status: Draft → Approved.

**Aksi:** `Add Purchase Order`, `Save`; per baris: `Detail`, `Edit`, `Delete`, `Approve` (Draft), `Convert` (Approved → invoice pembelian); di detail: `Print / PDF` + lampiran Upload/Download/Delete.

**Tabel `purchase_order_datatable`:** No, PO No, Order Date, Expected Date, Supplier, Status, Total, Action.

### 7.3 Goods Received (GRN)
**Fungsi:** Penerimaan barang dari PO **Approved** ke gudang. Status: Draft → Posted.

**Aksi:** `Add Goods Received`, `Save` (pilih PO + Warehouse); per baris: `Detail`, `Edit`, `Post` (Draft; menambah stok + jurnal inventory), `Delete` (Draft); filter status.

**Tabel `grn_datatable`:** No, GRN No, Receipt Date, PO No, Supplier, Warehouse, Status, Lines, Total, Action.
**Tabel item modal:** Ordered Qty, Remaining, Receive Qty, Unit Price, Tax, Line Total.

---

## 8. Inventory

Menu induk berisi 8 halaman pengelolaan barang & stok.

### 8.1 Items
**Fungsi:** Master daftar barang (kode, nama, tipe, grup, unit, harga). View-only.

**Aksi:** pencarian DataTable.

**Tabel `item_datatable`:** No, Item Code, Item Name, Type, Group, Unit, Sales Price, Purchase Price, Status.

### 8.2 Stock Card
**Fungsi:** Kartu stok — riwayat pergerakan stok masuk/keluar per item & gudang. View-only.

**Aksi:** pencarian DataTable.

**Tabel `stock_card_datatable`:** No, Date, Item, Warehouse, Type, In, Out, Unit Cost.

### 8.3 Stock Opname
**Fungsi:** Penghitungan fisik vs sistem per gudang (selisih = adjustment). Status: Draft → Posted.

**Aksi:** `Add Stock Opname`, `Load Items` (memuat qty sistem dari gudang), `Save`; per baris: `Detail`, `Edit`, `Post` (Draft; membuat adjustment stok), `Delete` (Draft).

**Tabel `opname_datatable`:** No, Opname No, Date, Warehouse, Status, Lines, Total Difference, Action.
**Tabel item modal:** Item Code, Item Name, Unit, System Qty (readonly), Actual Qty (input), Difference, Notes.

### 8.4 Item Groups
**Fungsi:** Master grup/kategori item.

**Aksi:** `Add Group`, `Save`; per baris: `Edit`, `Delete`.

**Tabel `group_datatable`:** No, Group Code, Group Name, Description, Items, Status, Action.

### 8.5 Units
**Fungsi:** Master satuan unit item.

**Aksi:** `Add Unit`, `Save`; per baris: `Edit`, `Delete`.

**Tabel `unit_datatable`:** No, Unit Code, Unit Name, Description, Status, Action.

### 8.6 Stock Transfer
**Fungsi:** Transfer/mutasi stok antar gudang. Status: Draft → Posted.

**Aksi:** `Add Transfer`, `Add Item`, `Save`; per baris: `Detail`, `Edit`, `Post` (Draft; memindah stok), `Delete` (Draft).

**Tabel `transfer_datatable`:** No, Transfer No, Date, From Warehouse, To Warehouse, Status, Lines, Total Qty, Action.

### 8.7 Stock Minimum
**Fungsi:** Monitoring stok terhadap reorder point (stok minimum).

**Aksi:** `Apply Filter` + checkbox **"Bawah reorder point saja"**; per baris: `Set Reorder Point` (modal).

**Tabel `stock_minimum_datatable`:** No, Item Code, Item Name, Unit, Current Stock, Reorder Point, Status, Action.

### 8.8 Serial Number / Batch
**Fungsi:** Registrasi & tracking batch/serial number item (termasuk expiry & sisa qty).

**Aksi:** `Register Batch / Serial` (modal: Item, Batch/Serial Number, Quantity, Expiry Date, Notes); per baris: `Consume` (kurangi qty batch) dan `Delete` (hanya status Available); filter per item.

**Tabel `serialbatch_datatable`:** No, Item Code, Item Name, Batch / Serial, Expiry, Qty, Remaining, Status (Available/Partial/Out), Action.

---

## 9. Approvals

**Fungsi:** Pusat persetujuan (workflow) dokumen yang disubmit dari transaksi. Status: Pending → Approved/Rejected.

**Aksi:** per baris `Approve` dan `Reject`.

**Tabel `approval_datatable`:** No, Document Type, Document ID, Status, Requested By, Requested At, Notes, Action.

---

## 10. Kas & Bank

Menu induk berisi 3 halaman.

### 10.1 Cash & Bank Accounts
**Fungsi:** Daftar akun kas & bank beserta saldo. View-only.

**Aksi:** `Manage COA` (link ke menu Chart of Accounts).

**Tabel:** No, Code, Account Name, Usage (Cash/Bank), Balance (+ footer Total Balance).

### 10.2 Cash Bank Transfers
**Fungsi:** Transfer dana antar akun kas/bank. Status: Draft → Posted.

**Aksi:** `Add Transfer`, `Save`; per baris: `Detail`, `Edit`, `Post` (Draft; membuat jurnal), `Delete` (Draft); filter status.

**Tabel `transfer_datatable`:** No, Transfer No, Date, From, To, Status, Amount, Action.

### 10.3 Bank Reconciliation
**Fungsi:** Rekonsiliasi bank — mencocokkan saldo statement bank vs saldo GL (menandai transaksi cleared/float).

**Aksi:** `Add Reconciliation`, `Load Statement`, `Add Line`, `Save`; per baris: `Detail`, `Edit`, `Post` (Draft), `Delete` (Draft). Ada preview keseimbangan (balanced / not balanced).

**Tabel `reconciliation_datatable`:** No, Recon No, Account, Statement Date, Statement Balance, GL Balance, Lines (Cleared), Status, Action.

---

## 11. HRM / Payroll

**Fungsi:** Mengelola data karyawan & penggajian. Terdiri dari 2 tab.

### 11.1 Data Karyawan
**Aksi:** `Add Employee`; per baris: Edit & Delete.

**Tabel `employee_datatable`:** No, Code, Employee Name, Position, Department, Hire Date, Basic Salary, Status, Action.

### 11.2 Penggajian
**Aksi:** `Buat Penggajian` (modal: Periode Bulan/Tahun, Payroll Date, Notes → buat Draft); per baris: `Detail` & Delete (draft); di detail: `Post Payroll` (jurnal otomatis Debit Salary Expense / Credit Payroll Payable).

**Tabel `payroll_datatable`:** No, Number, Period, Payroll Date, Employees, Net Salary, Journal, Status, Action.

---

## 12. Produksi

**Fungsi:** Manufaktur — mengelola Bill of Material (BOM) & Production Order. Terdiri dari 2 tab.

### 12.1 Bill of Material
**Aksi:** `Buat BOM` (pilih Finished Item + komponen `Add Line`); per baris: `Detail` & Delete.

**Tabel `bom_datatable`:** No, BOM Number, Finished Item, Components, Status, Action.

### 12.2 Production Order
**Aksi:** `Production Order` (modal: pilih BOM, Warehouse, Quantity, Production Date → Draft); per baris: `Detail` & Delete (draft); di detail: `Post Production` (mengurangi stok komponen, menambah stok barang jadi, membuat jurnal).

**Tabel `po_datatable`:** No, Order No, BOM, Finished Item, Qty, Date, Warehouse, Journal, Status, Action.

---

## Ringkasan Alur Status Dokumen

| Modul | Alur Status |
|-------|-------------|
| Journal Entry / Memo Journal | Draft → Posted → (Reverse) |
| Sales Quotation / Sales Order | Draft → Approved |
| Delivery Order | Draft → Posted → Invoiced |
| Purchase Request | Draft → Approved → Converted |
| Purchase Order | Draft → Approved → (Convert ke Invoice) |
| Goods Received | Draft → Posted |
| Invoice | Draft → Posted → Partially Paid → Paid / Cancelled |
| Payment / Receipt | Draft → Posted |
| Stock Opname / Stock Transfer | Draft → Posted |
| Payroll / Production | Draft → Posted |

> **Tips umum:**
> - Semua tabel memakai DataTables (pencarian, sorting, pagination) — arahkan kursor ke ikon kolom Action untuk melihat tombol yang tersedia.
> - Transaksi yang sudah **Posted** umumnya tidak bisa dihapus/diedit; gunakan aksi **Reverse** (untuk jurnal) atau lihat status berikutnya (invoice/payment).
> - Aksi **Approve/Post** menggunakan dialog konfirmasi.
