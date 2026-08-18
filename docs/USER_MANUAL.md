# User Manual — AccuFlow Accounting System

Panduan penggunaan aplikasi untuk semua user, ditulis dengan bahasa yang mudah dipahami.
Setiap modul dijelaskan mulai dari **apa fungsinya**, **kapan dipakai**, **tombol/aksi apa saja yang tersedia**, sampai **isi tabelnya**.

> **Istilah singkat yang sering muncul (baca dulu ya!):**
> - **Draft** — data baru yang masih "konsep"/belum sah. Masih bisa diedit dan dihapus.
> - **Posted** — data sudah disahkan/diposting. Artinya transaksi sudah dicatat resmi ke pembukuan (stok/jurnal), dan biasanya sudah **tidak bisa** diedit atau dihapus lagi.
> - **Reverse** — kebalikan dari posting: membuat jurnal baru dengan nilai berlawanan (misal debit jadi kredit) untuk "membatalkan" transaksi secara sah.
> - **Approved / Disetujui** — dokumen sudah lolos persetujuan atasan.
> - **Approve** — menyetujui sebuah dokumen.
> - **Reject** — menolak sebuah dokumen.
> - **Partner** — istilah umum untuk pelanggan (customer) atau pemasok (supplier).
> - **Outstanding** — sisa tagihan yang belum dibayar.
> - **Jurnal** — catatan pembukuan berpasangan debit (uang masuk/bertambah di akun) dan kredit (uang keluar/berkurang di akun). Setiap transaksi di aplikasi ini otomatis membuat jurnal.

---

## 1. Login

Buka aplikasi lalu masuk dengan akun Anda.

### User bawaan (default seed)

| Username      | Password           | Role / Hak Akses      |
|---------------|--------------------|-----------------------|
| `admin`       | `Admin123!`        | Super Administrator   |
| `administrator` | `Admin123!`      | Administrator         |
| `accountant`  | `Accountant123!`   | Accountant            |

> **Catatan:** Setelah login pertama, jika password sudah berumur lebih dari 1 bulan, sistem akan meminta Anda **mengganti password** dulu sebelum bisa lanjut.

### Halaman Akun
- **Login** — Masukkan Username atau Email + Password, lalu klik **Sign In**.
- **Lupa Password?** — Masukkan email terdaftar; instruksi reset dikirim ke email (jika layanan email sudah aktif).
- **Change Password** — Ganti password sendiri. Syarat: minimal 8 karakter, harus ada huruf besar, huruf kecil, angka, dan simbol.
- **Logout** — Keluar dari aplikasi.

---

## 2. Dashboard (Home)

Halaman pertama yang muncul setelah login — seperti "layar utama" aplikasi yang merangkum kondisi keuangan perusahaan.

**Fungsi:** memberi gambaran cepat kondisi keuangan dan aktivitas terbaru, tanpa perlu membuka laporan satu per satu.

Yang bisa Anda lihat / lakukan:
- **Sapaan** — "Selamat datang, [Nama]" beserta jabatan/role Anda.
- **Tombol pintasan cepat (hero):**
  - `New Journal` → langsung menuju halaman Journal Entry untuk mencatat jurnal baru.
  - `Open Invoices` → menuju daftar invoice.
  - `Ledger` → menuju General Ledger.
- **Quick Actions** — 6 tombol pintasan menu: Chart of Accounts, Invoices, Payments, Purchase Orders, User Access, Trial Balance.
- **Journal posting health** — lingkaran persentase: berapa banyak jurnal yang sudah diposting vs masih draft. Tujuannya agar pembukuan tidak menumpuk status draft.
- **Kartu metrik keuangan (format Rupiah):**
  - *Outstanding receivables* — total tagihan ke pelanggan yang belum dibayar (piutang).
  - *Outstanding payables* — total utang ke pemasok yang belum dibayar.
  - *Overdue invoices* — jumlah invoice yang sudah lewat jatuh tempo.
  - *Cash movement this month* — pergerakan kas masuk/keluar bulan ini.
- **Recent Activity** — daftar 6 aktivitas terakhir (jurnal & invoice) yang dilakukan.

---

## 3. User Management

Menu untuk mengatur siapa saja yang boleh masuk aplikasi dan apa saja yang boleh mereka lakukan. Berisi 2 halaman: **Users** dan **Roles**.

> **Cara kerja:** menu di sisi kiri tampil **menyesuaikan hak akses** Anda. User yang berbeda bisa melihat menu yang berbeda.

### 3.1 Users
**Fungsi:** mengelola akun pengguna — siapa yang bisa login, role-nya apa, dan apakah akunnya aktif/terkunci.

**Kapan dipakai:** ketika ada karyawan baru yang perlu akses aplikasi, atau akun perlu dinonaktifkan / dibuka kuncinya karena salah password berkali-kali.

**Aksi:**
- Toolbar: `Add New User` → membuat akun baru.
- Per baris: `View Detail`, `Edit`, `Delete`, `Unlock / Reset Password`, `Audit Trail`.
  - **Delete** hanya bisa untuk user **non-aktif** (user aktif harus dinonaktifkan dulu).
  - **Unlock / Reset Password** muncul jika akun terkunci (gagal login 3× berturut-turut). Password direset ke `Qwerty@123` dan user **wajib ganti password** saat login berikutnya.
  - **Audit Trail** — riwayat siapa yang membuat/mengubah data dan kapan.
- Filter: Role, Status (Active/Inactive), lalu **Apply Filter**; plus kotak pencarian.

**Tabel `user_datatable`:** No, Username, Email, Full Name, Role, Status (Locked/Active/Inactive), Action.

**Form Tambah/Edit:** Username, Email, Full Name, Role (dropdown), Status Aktif.

### 3.2 Roles
**Fungsi:** mengatur "peran" (role) dan hak aksesnya ke tiap menu (misal: role "Accountant" boleh akses Accounting tapi tidak boleh akses User Management).

**Kapan dipakai:** untuk membagi tugas dan keamanan — misal akuntan tidak boleh mengubah data user, admin yang pegang.

**Aksi:**
- Toolbar: `Add New Role`.
- Per baris: `View Detail`, `Edit Role`, `Manage Permissions` (ikon kunci), `Audit Trail`.

**Tabel `role_datatable`:** No, Role Name, Description, Status, Action.

**Assign Permission:** centang menu dan aksi yang boleh dilakukan role tersebut — `View` (lihat), `Add` (tambah), `Edit` (ubah), `Delete` (hapus), `Post` (posting), `Reverse` (membalik jurnal). Ada tombol **Select All** untuk memberikan akses penuh sekaligus.

**Role Type bawaan:** Super Administrator, Administrator, Manager, Accountant, Finance Staff, AR Officer, AP Officer, Purchasing, Sales, Warehouse, Viewer.

---

## 4. Master

Menu untuk mengisi "data pokok" (master data) yang menjadi acuan transaksi sehari-hari. Berisi: **Chart of Accounts**, **Customers**, **Suppliers**.

> **Logika sederhana:** seperti buku alamat dan kode-kode dasar. Kalau master datanya benar, semua transaksi di bawahnya otomatis benar.

### 4.1 Chart of Accounts (COA)
**Fungsi:** daftar lengkap "kotak-kotak" pencatatan keuangan (akun) — misal Kas, Bank, Piutang, Utang, Pendapatan, Beban. Akun disusun bertingkat (parent–child), misal "Current Assets" punya anak "Cash", "Cash in Bank", dll.

**Kapan dipakai:** menentukan ke mana tiap transaksi dicatat. Ini fondasi seluruh pembukuan.

**Aksi:**
- Toolbar: `Add Account`, `Download Template`, `Export Excel`, `Import`.
- Per baris: `View Detail`, `Edit`, `Delete`, `Toggle Status` (Aktif/Nonaktif).
- Filter: Account Type, Status, lalu **Apply Filter**.

**Tabel:** No, Account Code, Account Name, Type, Parent, Status, Action.

**Download Template & Import:**
- **Download Template** → mengunduh file Excel (`.xlsx`) berisi 2 sheet: `ChartOfAccounts` (kolom contoh) dan `Instructions` (panduan tiap kolom). Kolom template: **Account Code, Account Name, Account Type, Parent Account Code, Description, Is Header, Is Active, Opening Balance, Currency**.
- **Import** → memilih file `.xlsx` untuk diunggah. **Syarat wajib: jumlah kolom dan nama header harus sama persis dengan template.** Kalau tidak cocok, muncul alert error dan import dibatalkan.
- Aturan isi per baris:
  - `Account Code` (wajib, unik), `Account Name` (wajib, 3–255 karakter), `Account Type` (salah satu: Asset, Liability, Equity, Revenue, Expense, Other Income, Other Expense).
  - `Parent Account Code` (opsional) — kode akun induk; harus sudah ada di sistem atau ditulis di baris yang lebih atas pada file yang sama. Kosongkan untuk akun root.
  - `Is Header` & `Is Active` diisi `Yes`/`No`.
  - `Currency` default `IDR` jika dikosongkan.
- Hasil import ditampilkan: jumlah berhasil diimpor dan jumlah baris yang dilewati karena error, lengkap dengan detail error per baris.

### 4.2 Customers
**Fungsi:** data pelanggan — identitas, kontak, alamat, dan syarat kredit.

**Kapan dipakai:** pelanggan baru / perubahan data, sebelum membuat quotation atau invoice.

**Aksi:**
- Toolbar: `Add Customer`, `Export Excel`.
- Per baris: `View Detail`, `Edit`, `Delete`, `Toggle Status`.
- Filter: Customer Type (Individual/Corporate/Government), Status.

**Tabel `customer_datatable`:** No, Customer Code, Customer Name, Type, Contact Person, Phone, Email, Status, Action.

**Form (5 tab):**
1. **Basic Info** — kode (otomatis `CUST-XXXXX`), tipe, nama, aktif.
2. **Contact** — Contact Person, Phone, Email, Website.
3. **Address** — alamat lengkap (city, state, postal code, country).
4. **Financial** — Credit Limit (batas kredit), Payment Terms (jatuh tempo, default 30 hari), Tax ID.
5. **Additional** — catatan.

### 4.3 Suppliers
**Fungsi:** data pemasok — sama persis penggunaannya dengan Customers, tapi untuk pihak yang menjual barang ke perusahaan Anda.

**Aksi:**
- Toolbar: `Add Supplier`, `Export Excel`.
- Per baris: `View Detail`, `Edit`, `Delete`, `Toggle Status`.
- Filter: Supplier Type, Status.

**Tabel `supplier_datatable`:** No, Supplier Code, Supplier Name, Type, Contact Person, Phone, Email, Status, Action.

---

## 5. Accounting

Menu inti pembukuan — berisi 13 halaman untuk mencatat transaksi dan melihat laporan keuangan.

> **Alur umum pembukuan:** catat transaksi (jurnal/invoice/payment) → posting agar sah → lihat hasilnya di laporan (GL, Trial Balance, Laporan Keuangan).

### 5.1 Journal Entry
**Fungsi:** mencatat jurnal umum manual — transaksi yang dicatat langsung secara debit/kredit, misal penyesuaian manual atau transaksi di luar modul lain.

**Kapan dipakai:** untuk mencatat transaksi yang tidak ada form khususnya (misal pembayaran sewa tunai, koreksi, setoran modal).

**Aksi:**
- Toolbar: `Add Journal`, `Export Excel`.
- Per baris: `View Detail`, `Edit` (hanya saat Draft).
- Di jendela detail: `Post Journal` (mengesahkan), `Reverse Journal` (membatalkan secara sah).
- Filter: rentang tanggal, status (Draft/Posted/Reversed), akun.

**Tabel:** No, Journal Number, Date, Description, Total Amount, Status, Action.

> **Catatan:** jurnal yang sudah Posted tidak bisa dihapus. Kalau salah, gunakan **Reverse** agar jejak audit tetap tercatat.

### 5.2 General Ledger (Buku Besar)
**Fungsi:** merangkum semua mutasi per akun dalam satu periode — berapa total debit, total kredit, dan saldo akhir setiap akun.

**Kapan dipakai:** untuk menelusuri pergerakan satu akun tertentu (misal akun "Kas di Bank") — klik `View Ledger` untuk melihat transaksi per transaksinya.

**Aksi:** `Export Summary`; per baris: `View Ledger`; filter: tanggal, tipe akun.

**Tabel ringkasan:** Account Code, Account Name, Type, Total Debit, Total Credit, Balance, Transactions, Actions.
**Tabel Account Ledger:** Date, Journal Number, Description, Debit, Credit, Balance (dengan Saldo Awal & Saldo Akhir).

### 5.3 Trial Balance (Neraca Saldo)
**Fungsi:** "pemeriksaan kesehatan" pembukuan — daftar semua akun dan saldonya pada tanggal tertentu, dikelompokkan per tipe, dengan total debit vs kredit.

**Kapan dipakai:** rutin (misal bulanan) untuk memastikan pembukuan seimbang (total debit = total kredit) sebelum menyusun laporan keuangan.

**Aksi:** `Export Excel`; filter: **As of Date**, Account Type, centang **Show Zero Balance** (tampilkan akun bersaldo nol).

**Tabel:** Account Code, Account Name, Debit, Credit (ada baris subtotal per grup dan baris TOTAL). Jika tidak seimbang muncul badge **NOT BALANCED** + selisihnya.

### 5.4 Financial Statements (Laporan Keuangan)
**Fungsi:** laporan keuangan resmi dalam 3 tab:
- **Income Statement** (Laba/Rugi) — pendapatan dikurangi beban dalam satu periode.
- **Balance Sheet** (Neraca) — aset, utang, dan modal pada tanggal tertentu.
- **Cash Flow** (Arus Kas) — pergerakan kas masuk/keluar.

**Kapan dipakai:** akhir periode (bulanan/tahunan) untuk manajemen dan pelaporan.

**Aksi:** `Export Excel` (mengekspor tab yang sedang aktif); filter: periode/tanggal + Show Zero Balance.

**Tabel:** Account Code, Account Name, Amount.

### 5.5 Invoices (Faktur)
**Fungsi:** membuat dan mengelola faktur penjualan (Sales) dan pembelian (Purchase) — tagihan ke pelanggan / dari pemasok, lengkap dengan jatuh tempo dan sisa tagihan (Outstanding).

**Kapan dipakai:** setelah barang dikirim (Delivery Order) atau barang diterima, invoice dibuat dan diposting untuk menagih / dicatat utang.

**Aksi:**
- Toolbar: `Add Invoice`.
- Per baris: `Detail`, `Edit`, `Delete` (Draft), `Post` (Draft).
- Di detail: `Print / PDF`, lampiran `Upload / Download / Delete` (misal scan PO, surat jalan).
- Filter: tipe (Sales/Purchase), status (Draft/Posted/Partially Paid/Paid/Cancelled).

**Tabel:** No, Invoice No, Type, Date, Due Date, Partner, Status, Total, Outstanding, Action.

### 5.6 Payments & Receipts (Pembayaran & Penerimaan)
**Fungsi:** mencatat pembayaran keluar (Payment) dan penerimaan masuk (Receipt), lalu **dialokasikan ke invoice tertentu** — sehingga sisa tagihan (Outstanding) otomatis berkurang.

**Kapan dipakai:** ketika pelanggan membayar invoice (Receipt) atau perusahaan membayar utang ke pemasok (Payment). Metode pembayaran: Bank Transfer, Cash, Giro.

**Aksi:**
- Toolbar: `Add Payment`.
- Per baris: `Detail`, `Edit`, `Delete` (Draft), `Post` (Draft).
- Di detail: `Print / PDF`, lampiran.
- Filter: Type (Receipt/Payment), Status.

**Tabel:** No, Payment No, Type, Date, Partner, Method, Status, Total, Action.

### 5.7 Aging Report (Laporan Umur Piutang/Utang)
**Fungsi:** mengelompokkan tagihan berdasarkan "umur" keterlambatan — berapa yang masih dalam tenggat (Current), lewat 1–30 hari, 31–60, 61–90, dan lebih dari 90 hari.

**Kapan dipakai:** untuk menilai risiko penagihan (piutang) dan perencanaan pembayaran (utang).

**Aksi:** pilih **Aging Type** (AR Aging = piutang, AP Aging = utang) + **As Of Date**, lalu `Generate`.

**Tabel:** Partner Code, Partner Name, Current, 1-30, 31-60, 61-90, >90, Total.

### 5.8 Returns (Retur)
**Fungsi:** mencatat pengembalian barang — retur penjualan (customer mengembalikan barang ke kita) atau retur pembelian (kita mengembalikan barang ke supplier) terhadap invoice.

**Kapan dipakai:** ada barang rusak/tidak sesuai yang dikembalikan. Saat **Post**, stok otomatis berkurang dan jurnal dibuat.

**Aksi:**
- Toolbar: `Add Return`.
- Per baris: `Detail`, `Edit`, `Delete` (Draft), `Post` (Draft).
- Filter: Return Type, Status.

**Tabel:** No, Return No, Type, Date, Invoice, Partner, Status, Lines, Total, Action.
**Tabel item detail:** Item, Description, Invoice Qty, Remaining, Return Qty, Unit Price, Tax, Line Total.

### 5.9 Receivable & Payable (Piutang & Utang)
**Fungsi:** laporan rinci piutang (AR) atau utang (AP) **per invoice** — berapa nilai total, sudah terbayar berapa, dan sisa berapa, plus status Overdue/Open.

**Kapan dipakai:** untuk mengetahui secara detail tagihan mana yang sudah/belum dibayar.

**Aksi:** pilih **Report Type** (Receivable AR / Payable AP) + **As Of Date**, lalu `Generate`.

**Tabel:** Partner Code, Partner Name, Invoice No, Invoice Date, Due Date, Total, Paid, Outstanding, Status.

### 5.10 Taxes (Pajak)
**Fungsi:** dua hal dalam satu halaman:
1. **Tax List** — master tarif pajak (VAT/PPN, Withholding, Other).
2. **PPN Report** — laporan PPN periodik: PPN Keluaran (dari penjualan), PPN Masukan (dari pembelian), dan PPN Bersih.

**Kapan dipakai:** mengatur tarif pajak yang dipakai di transaksi dan merekap PPN untuk pelaporan.

**Aksi:** tab Tax List: `Add Tax`, `Edit`, `Delete`; tab PPN Report: pilih **From/To Date** → `Generate` (muncul kartu PPN Keluaran/Masukan/Bersih).

**Tabel Tax List:** No, Tax Code, Tax Name, Rate (%), Type, Status, Action.
**Tabel PPN Report:** Invoice No, Type, Partner, Date, DPP, PPN, Total.

### 5.11 Fixed Assets (Aset Tetap)
**Fungsi:** mengelola aset perusahaan (peralatan, kendaraan, bangunan, dll.) — nilai perolehan, nilai sisa, umur manfaat, dan akumulasi penyusutan (depresiasi), sehingga bisa dilihat **Net Book Value** (nilai aset setelah penyusutan).

**Kapan dipakai:** saat membeli aset baru, atau saat melakukan penyusutan berkala.

**Aksi:**
- Toolbar: `Add Asset`.
- Per baris: `Detail`, `Edit`, `Delete`, **`Depreciate`** (untuk aset berstatus Active) → muncul modal **Run Depreciation**, isi **Period Date**, klik `Run` → sistem membuat jurnal otomatis: Debit Depreciation Expense / Credit Accumulated Depreciation.

**Tabel:** No, Asset Code, Asset Name, Category, Purchase Date, Cost, Accum. Depr., Net Book Value, Status, Action.

### 5.12 Year-End Closing (Penutupan Tahun)
**Fungsi:** menutup tahun buku — semua akun pendapatan dan beban "ditutup" (dipindahkan) ke akun Retained Earnings (laba ditahan) lewat jurnal otomatis, lalu dibuka periode baru dengan saldo nol di akun pendapatan/beban.

**Kapan dipakai:** sekali setahun di akhir periode fiskal. **Mohon berhati-hati** — proses ini memengaruhi seluruh pembukuan.

**Aksi:** pilih **Fiscal Year** → klik `Preview` (lihat ringkasan & akun yang akan ditutup) → klik `Close Fiscal Year` (ada konfirmasi). Ditampilkan juga **Riwayat Penutupan**.

**Kartu ringkasan:** Total Revenue, Total Expense, Net Income.
**Tabel preview akun:** Account Code, Account Name, Balance.
**Tabel Riwayat Penutupan:** Fiscal Year, Closing Date, Journal No.

### 5.13 Jurnal Memo / Penyesuaian
**Fungsi:** mencatat jurnal memo / penyesuaian manual dengan tipe **Memo** (catatan) atau **Adjustment** (koreksi/pemindahbukuan), misal koreksi akhir periode.

**Kapan dipakai:** untuk koreksi kecil atau jurnal penyesuaian yang tidak lewat modul lain.

**Aksi:**
- Toolbar: `Add Journal`.
- Per baris: `View Detail`, `Edit` (Draft).
- Di detail: `Post Journal`, `Reverse Journal`.
- Filter: tanggal, tipe, status.

**Tabel:** No, Journal Number, Date, Description, Total Amount, Status, Action.

---

## 6. Sales (Penjualan)

Menu alur penjualan lengkap: **Quotation (penawaran) → Order (pesanan) → Delivery (pengiriman)**.

> **Alur baca:** Penawaran ke pelanggan → disetujui → jadi pesanan → disetujui → dikirim → (baru kemudian dibuat invoice-nya di menu Accounting).

### 6.1 Sales Quotation (Penawaran)
**Fungsi:** membuat penawaran harga resmi ke pelanggan (surat penawaran berisi barang, jumlah, harga, masa berlaku).

**Kapan dipakai:** tahap awal sebelum pelanggan memutuskan membeli.

**Aksi:**
- Toolbar: `Add Quotation`, `Add Line` (tambah baris barang), `Save`.
- Per baris: `Detail`, `Edit`, `Approve`, `Delete` — tombol **Approve/Delete hanya muncul saat status Draft**.
- Filter: status (Draft/Approved).

**Tabel `quote_datatable`:** No, Quote No, Date, Valid Until, Customer, Status, Lines, Total, Action.
**Tabel item detail:** Item, Description, Qty, Unit Price, Discount, Tax, Line Total.

### 6.2 Sales Order (Pesanan Penjualan)
**Fungsi:** mencatat pesanan pembelian dari pelanggan. Bisa dibuat dari Quotation yang sudah **Approved** (dropdown "From Quotation" otomatis mengisi baris barang).

**Kapan dipakai:** setelah pelanggan setuju dengan penawaran.

**Aksi:**
- Toolbar: `Add Sales Order`, `Add Line`, `Save`.
- Per baris: `Detail`, `Edit`, `Approve`, `Delete` (saat Draft).
- Filter: status.

**Tabel `order_datatable`:** No, Order No, Order Date, Expected, Customer, Quote, Status, Lines, Total, Action.

### 6.3 Delivery Order (Surat Jalan / Pengiriman)
**Fungsi:** mencatat pengiriman barang ke pelanggan berdasarkan Sales Order yang sudah **Approved**.

**Kapan dipakai:** saat barang benar-benar dikirim. Saat **Post**, stok berkurang otomatis dan jurnal inventory dibuat.

**Aksi:**
- Toolbar: `Add Delivery Order`, `Save`.
- Per baris: `Detail`, `Edit`, `Post` (Draft), `Delete` (Draft), **`To Invoice`** (saat status Posted → mengubahnya menjadi invoice penjualan di menu Invoices).
- Filter: status (Draft/Posted/Invoiced).

**Tabel `delivery_datatable`:** No, Delivery No, Date, Sales Order, Customer, Status, Lines, Total, Action.
**Tabel item modal:** Ordered Qty, Remaining, Deliver Qty, Unit Price, Line Total.

---

## 7. Purchasing (Pembelian)

Menu alur pembelian lengkap: **Purchase Request (permintaan) → Purchase Order (pesanan) → Goods Received (penerimaan barang)**.

> **Alur baca:** bagian yang butuh barang membuat permintaan → disetujui → dibuatkan pesanan ke supplier → disetujui → barang diterima ke gudang → (baru dibuat invoice pembeliannya).

### 7.1 Purchase Request (Permintaan Pembelian)
**Fungsi:** permintaan internal untuk membeli barang — siapa yang minta, dari departemen apa, barang apa, berapa banyak.

**Kapan dipakai:** departemen membutuhkan barang; ini "surat usul" sebelum benar-benar dipesan.

**Aksi:**
- Toolbar: `Add Purchase Request`, `Add Line`, `Save`.
- Per baris: `Detail`, `Edit`, `Approve`, `Delete` (Draft), **`Convert`** (saat status Approved → pilih **Supplier** + **Expected Date** untuk diteruskan ke Purchase Order).
- Filter: status (Draft/Approved/Converted).

**Tabel `pr_datatable`:** No, PR No, Request Date, Required, Requested By, Department, Status, Lines, Total, PO, Action.

### 7.2 Purchase Orders (Pesanan Pembelian)
**Fungsi:** pesanan resmi ke supplier (PO). Status: Draft → Approved.

**Kapan dipakai:** setelah permintaan disetujui, PO diterbitkan sebagai bukti pesanan ke supplier.

**Aksi:**
- Toolbar: `Add Purchase Order`, `Save`.
- Per baris: `Detail`, `Edit`, `Delete`, `Approve` (Draft), **`Convert`** (saat Approved → diteruskan menjadi invoice pembelian).
- Di modal Detail: `Print / PDF` + lampiran `Upload / Download / Delete`.

**Tabel `purchase_order_datatable`:** No, PO No, Order Date, Expected Date, Supplier, Status, Total, Action.

### 7.3 Goods Received / GRN (Penerimaan Barang)
**Fungsi:** mencatat barang yang benar-benar diterima ke gudang berdasarkan PO yang sudah **Approved**.

**Kapan dipakai:** saat supplier mengirim barang. Saat **Post**, stok bertambah otomatis dan jurnal inventory dibuat.

**Aksi:**
- Toolbar: `Add Goods Received`, `Save` (pilih **PO** + **Warehouse**).
- Per baris: `Detail`, `Edit`, `Post` (Draft), `Delete` (Draft).
- Filter: status.

**Tabel `grn_datatable`:** No, GRN No, Receipt Date, PO No, Supplier, Warehouse, Status, Lines, Total, Action.
**Tabel item modal:** Ordered Qty, Remaining, Receive Qty, Unit Price, Tax, Line Total.

---

## 8. Inventory (Gudang / Persediaan)

Menu untuk mengelola barang dan stok — berisi 8 halaman.

### 8.1 Items (Barang)
**Fungsi:** daftar semua barang — kode, nama, tipe, grup, unit, harga jual & harga beli, status. Halaman ini hanya untuk **melihat** daftar barang.

**Aksi:** pencarian DataTable (ketik untuk mencari).

**Tabel `item_datatable`:** No, Item Code, Item Name, Type, Group, Unit, Sales Price, Purchase Price, Status.

### 8.2 Stock Card (Kartu Stok)
**Fungsi:** riwayat pergerakan stok satu barang — kapan masuk, kapan keluar, berapa jumlahnya, di gudang mana. View-only.

**Kapan dipakai:** untuk audit stok atau menelusuri kenapa stok berkurang.

**Aksi:** pencarian DataTable.

**Tabel `stock_card_datatable`:** No, Date, Item, Warehouse, Type, In, Out, Unit Cost.

### 8.3 Stock Opname (Stok Fisik)
**Fungsi:** mencocokkan **stok sistem** dengan **stok fisik** hasil hitungan di lapangan per gudang. Selisihnya menjadi penyesuaian (adjustment) stok.

**Kapan dipakai:** saat audit stok rutin (misal akhir bulan/tahun).

**Aksi:**
- Toolbar: `Add Stock Opname`, **`Load Items`** (otomatis memuat stok sistem dari gudang), `Save`.
- Per baris: `Detail`, `Edit`, `Post` (Draft; membuat adjustment stok), `Delete` (Draft).

**Tabel `opname_datatable`:** No, Opname No, Date, Warehouse, Status, Lines, Total Difference, Action.
**Tabel item modal:** Item Code, Item Name, Unit, System Qty (tidak bisa diubah), Actual Qty (isi hasil hitungan), Difference, Notes.

### 8.4 Item Groups (Grup Barang)
**Fungsi:** mengelompokkan barang berdasarkan kategori (misal: Bahan Baku, Barang Jadi, Spare Part).

**Aksi:** `Add Group`, `Save`; per baris: `Edit`, `Delete`.

**Tabel `group_datatable`:** No, Group Code, Group Name, Description, Items, Status, Action.

### 8.5 Units (Satuan)
**Fungsi:** daftar satuan barang (pcs, box, kg, liter, dll.).

**Aksi:** `Add Unit`, `Save`; per baris: `Edit`, `Delete`.

**Tabel `unit_datatable`:** No, Unit Code, Unit Name, Description, Status, Action.

### 8.6 Stock Transfer (Mutasi Stok)
**Fungsi:** memindahkan stok antar gudang (misal dari gudang pusat ke cabang).

**Aksi:**
- Toolbar: `Add Transfer`, `Add Item`, `Save`.
- Per baris: `Detail`, `Edit`, `Post` (Draft; stok pindah dari gudang asal ke gudang tujuan), `Delete` (Draft).

**Tabel `transfer_datatable`:** No, Transfer No, Date, From Warehouse, To Warehouse, Status, Lines, Total Qty, Action.

### 8.7 Stock Minimum (Stok Minimum)
**Fungsi:** memantau stok agar tidak kehabisan — membandingkan **stok sekarang** dengan **reorder point** (batas minimal). Barang yang di bawah batas ditandai.

**Kapan dipakai:** untuk tahu barang mana yang harus segera dibeli ulang.

**Aksi:** `Apply Filter` + centang **"Bawah reorder point saja"**; per baris: `Set Reorder Point` (mengubah batas minimal barang).

**Tabel `stock_minimum_datatable`:** No, Item Code, Item Name, Unit, Current Stock, Reorder Point, Status, Action.

### 8.8 Serial Number / Batch
**Fungsi:** melacak barang berdasarkan nomor seri atau nomor batch — penting untuk barang dengan tanggal kedaluwarsa atau yang harus bisa ditelusuri (traceability).

**Kapan dipakai:** barang farmasi, makanan, elektronik berseri, dll.

**Aksi:**
- Toolbar: `Register Batch / Serial` (isi: Item, Batch/Serial Number, Quantity, Expiry Date, Notes).
- Per baris: `Consume` (mengurangi jumlah batch yang tersedia) dan `Delete` (hanya jika status Available).
- Filter: per item.

**Tabel `serialbatch_datatable`:** No, Item Code, Item Name, Batch / Serial, Expiry, Qty, Remaining, Status (Available/Partial/Out), Action.

---

## 9. Approvals (Persetujuan)

**Fungsi:** satu tempat untuk menyetujui atau menolak dokumen yang dikirimkan untuk persetujuan (workflow) — misal PO, quotation, atau transaksi lain yang butuh tanda tangan atasan.

**Kapan dipakai:** ketika ada dokumen yang statusnya Pending dan menunggu keputusan Anda (sesuai role).

**Aksi:** per baris klik `Approve` (setujui) atau `Reject` (tolak).

**Tabel `approval_datatable`:** No, Document Type, Document ID, Status, Requested By, Requested At, Notes, Action.

---

## 10. Kas & Bank

Menu untuk mengelola uang tunai (kas) dan rekening bank — berisi 3 halaman.

### 10.1 Cash & Bank Accounts (Akun Kas & Bank)
**Fungsi:** daftar akun kas dan bank beserta saldo masing-masing. View-only.

**Aksi:** `Manage COA` (pintasan ke Chart of Accounts untuk mengatur akunnya).

**Tabel:** No, Code, Account Name, Usage (Cash/Bank), Balance (+ footer Total Balance).

### 10.2 Cash Bank Transfers (Transfer Kas/Bank)
**Fungsi:** memindahkan dana antar akun kas/bank (misal menarik tunai dari bank ke kas kecil).

**Aksi:**
- Toolbar: `Add Transfer`, `Save`.
- Per baris: `Detail`, `Edit`, `Post` (Draft; membuat jurnal), `Delete` (Draft).
- Filter: status.

**Tabel `transfer_datatable`:** No, Transfer No, Date, From, To, Status, Amount, Action.

### 10.3 Bank Reconciliation (Rekonsiliasi Bank)
**Fungsi:** mencocokkan **saldo mutasi bank** (dari statement bank) dengan **saldo pembukuan (GL)** di aplikasi, dengan menandai transaksi mana yang sudah masuk (cleared) dan mana yang masih mengambang (float).

**Kapan dipakai:** rutin (misal bulanan) agar pembukuan kas/bank selalu sesuai dengan kenyataan di bank.

**Aksi:**
- Toolbar: `Add Reconciliation`, **`Load Statement`** (memuat transaksi bank), `Add Line`, `Save`.
- Per baris: `Detail`, `Edit`, `Post` (Draft), `Delete` (Draft).
- Ada preview keseimbangan: **balanced** (seimbang) atau **not balanced**.

**Tabel `reconciliation_datatable`:** No, Recon No, Account, Statement Date, Statement Balance, GL Balance, Lines (Cleared), Status, Action.

---

## 11. HRM / Payroll (Kepegawaian & Gaji)

**Fungsi:** mengelola data karyawan dan perhitungan/pembayaran gaji. Terdiri dari 2 tab.

### 11.1 Data Karyawan
**Fungsi:** data karyawan — kode, nama, posisi, departemen, tanggal mulai kerja, gaji pokok.

**Aksi:** `Add Employee`; per baris: Edit & Delete.

**Tabel `employee_datatable`:** No, Code, Employee Name, Position, Department, Hire Date, Basic Salary, Status, Action.

### 11.2 Penggajian
**Fungsi:** membuat penggajian bulanan — sistem menghitung total gaji karyawan, lalu saat **Post Payroll** dibuat jurnal otomatis (Debit Salary Expense / Credit Payroll Payable).

**Kapan dipakai:** setiap periode penggajian (biasanya bulanan).

**Aksi:**
- Toolbar: `Buat Penggajian` (isi: Periode Bulan/Tahun, Payroll Date, Notes → dibuat sebagai Draft).
- Per baris: `Detail` (lihat rincian) & Delete (draft).
- Di modal Detail: `Post Payroll`.

**Tabel `payroll_datatable`:** No, Number, Period, Payroll Date, Employees, Net Salary, Journal, Status, Action.

---

## 12. Produksi (Manufaktur)

**Fungsi:** mengelola proses produksi — menentukan bahan baku yang dibutuhkan (BOM) dan membuat perintah produksi. Terdiri dari 2 tab.

### 12.1 Bill of Material (BOM / Resep Produksi)
**Fungsi:** "resep" sebuah barang jadi — barang apa yang dihasilkan, terdiri dari komponen apa saja, dan berapa jumlah tiap komponen per unit.

**Kapan dipakai:** saat pertama kali mendefinisikan produk manufaktur.

**Aksi:** `Buat BOM` (pilih Finished Item + komponen via `Add Line`); per baris: `Detail` & Delete.

**Tabel `bom_datatable`:** No, BOM Number, Finished Item, Components, Status, Action.

### 12.2 Production Order (Perintah Produksi)
**Fungsi:** perintah untuk benar-benar memproduksi sejumlah barang jadi menggunakan BOM.

**Kapan dipakai:** saat memulai proses produksi. Saat **Post Production**, stok komponen berkurang, stok barang jadi bertambah, dan jurnal dibuat.

**Aksi:**
- Toolbar: `Production Order` (isi: pilih BOM, Warehouse, Quantity, Production Date → dibuat Draft).
- Per baris: `Detail` & Delete (draft).
- Di modal Detail: `Post Production`.

**Tabel `po_datatable`:** No, Order No, BOM, Finished Item, Qty, Date, Warehouse, Journal, Status, Action.

---

## Ringkasan Alur Status Dokumen

| Modul | Alur Status | Artinya |
|-------|-------------|---------|
| Journal Entry / Memo Journal | Draft → Posted → (Reverse) | catat dulu, lalu sahkan; kalau salah bisa dibalik |
| Sales Quotation / Sales Order | Draft → Approved | buat dulu, lalu disetujui |
| Delivery Order | Draft → Posted → Invoiced | buat → kirim (stok berkurang) → jadi invoice |
| Purchase Request | Draft → Approved → Converted | minta → setuju → jadi PO |
| Purchase Order | Draft → Approved | buat → setuju |
| Goods Received | Draft → Posted | terima barang → stok bertambah |
| Invoice | Draft → Posted → Partially Paid → Paid / Cancelled | faktur → sah → sebagian dibayar → lunas / batal |
| Payment / Receipt | Draft → Posted | catat pembayaran → sah |
| Stock Opname / Stock Transfer | Draft → Posted | hitung/transfer → stok disesuaikan |
| Payroll / Production | Draft → Posted | buat → sahkan (jurnal dibuat) |

> **Tips umum:**
> - Semua tabel memakai DataTables — Anda bisa **mencari** (kotak pencarian), **mengurutkan** (klik judul kolom), dan **membalik halaman**.
> - Arahkan kursor ke kolom **Action** untuk melihat tombol yang tersedia di tiap baris.
> - Transaksi yang sudah **Posted** umumnya tidak bisa diedit/dihapus. Untuk jurnal gunakan **Reverse**; untuk invoice/payment biarkan mengalir ke status berikutnya.
> - Tombol **Approve/Post** selalu ada dialog konfirmasi — baca dulu sebelum klik.
> - Jika ragu melakukan sesuatu yang berdampak besar (misal Year-End Closing, Posting, Delete), konsultasikan dulu dengan atasan/admin.
