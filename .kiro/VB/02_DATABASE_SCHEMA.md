# DATABASE SCHEMA

## Connection String
```
Data Source: 103.82.242.240 (Remote Server)
Initial Catalog: SPBE / Elpiji
Authentication: SQL Server (sa user)
```

## Core Tables

### 1. MASTER DATA TABLES

#### Profile (Company/PT)
- ProfileId (PK)
- Nama, Alamat, Kota, Telp, NPWP, Email
- Type (Agen/Distributor)
- Audit Fields: CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate

#### Customer (Delivery Point/Pangkalan)
- CustomerId (PK)
- Nama, Alamat, Kota, Telp, NPWP, Email
- Grup, NamaPT (FK to Profile)
- Audit Fields

#### MsSUPPLYPOINT (Supply Point)
- SPID (PK)
- Nama, Alamat, Kota, Telp, NPWP, Email
- Grup, NamaPT (FK to Profile)
- Audit Fields

#### Supplier
- SupplierId (PK)
- Nama, Alamat, Kota, Telp, NPWP, Email
- NamaPT (FK to Profile)
- Audit Fields


#### Driver
- DriverId (PK)
- Nama, Alamat, Telpon
- StatusSupir, BiayaSewa
- NamaPT (FK to Profile)
- Audit Fields

#### Car (Mobil)
- CarId (PK)
- CodeMobil, JenisMobil, NoPol, Kapasitas
- TahunMobil, WarnaMobil
- BiayaSewa, StatusMobil
- NamaPT (FK to Profile)
- Audit Fields

#### Barang (Product - LPG)
- BarangId (PK)
- Nama, Jenis, Satuan
- Harga1 (Harga Beli), Harga2 (Harga Jual)
- Qty (Stock)
- NamaPT (FK to Profile)
- Audit Fields

#### Pengguna (User)
- UserId (PK)
- Nama, NamaPanjang
- Password (Hashed with PBKDF2)
- Salt (for password hashing)
- Status (Role: Admin, Report, Administrator, SuperAdministrator)
- Audit Fields


### 2. TRANSACTION TABLES

#### BELIKIDUNG (Purchase Transactions)
- BeliId (PK, GUID)
- FakturBeliId (FK to FAKTURBELIKIDUNG)
- BarangId, Nama, JENIS
- HargaBeli, Qty, Total
- DriverId, NamaDriver
- CarId, CodeMobil
- TransactionDate
- PT (Company), NamaSP (Supply Point)
- Keterangan (Notes)
- Audit Fields

#### FAKTURBELIKIDUNG (Purchase Invoice Header)
- FakturBeliId (PK, Format: KDBUOM-00000001)
- ProfileId, NamaPT
- SPID, NamaSP (Supply Point)
- TotalJual (Unit Price), Totalqty, GrantTotal
- VadlidFrom (Transaction Date)
- SoldTo, ShipTo, Schdafrmt
- Audit Fields

#### TEMPBELIKIDUNG (Temporary Purchase - for data entry)
- TempId (PK, GUID)
- Similar structure to BELIKIDUNG
- Used for staging before final save


#### JUALKIDUNG (Sales Transactions)
- JualId (PK, GUID)
- FakturJualId (FK to FAKTURKIDUNG)
- BarangId, Nama, JENIS
- HargaBeli, HargaJual
- Qty, TotalBeli, Total
- DriverId, NamaDriver
- CarId, CodeMobil
- TransactionDate
- PT (Company)
- Keterangan (Notes)
- Audit Fields

#### FAKTURKIDUNG (Sales Invoice Header)
- FakturJualId (PK, Format: KDJUOM-00000001)
- InvoiceNo (Format: INVJKDUOM-00000001)
- ProfileId, NamaPT
- SdDP (Customer ID), NamaDP (Customer Name)
- TotalBeli, TotalJual (Unit Prices)
- Totalqty, GrantTotalBeli, GrantTotal
- TransactionDate
- Status (Cash/Credit)
- SoldTo (Customer Code)
- Audit Fields

### 3. ACCOUNTING TABLES

#### COA (Chart of Accounts)
- COAId (PK)
- Category1, Category2
- NamaAkun (Account Name)
- Audit Fields


#### KELOMPOKAKUN (Account Groups)
- KelompokAkunId (PK)
- Nilai (Code), Nama (Name)
- KelompokLaporan (Report Group)
- Audit Fields

#### KASTRANSAKSI (Cash Transactions)
- KasId (PK)
- PT (Company)
- ValidDate (Transaction Date)
- COAId (FK to COA)
- Debet, Kredit
- Keterangan (Description)
- Audit Fields

#### TEMPKAS (Temporary Cash - for journal entry)
- Similar to KASTRANSAKSI
- Used for staging before posting

#### TEMPTRANSAKSI (Temporary Transactions)
- Similar to KASTRANSAKSI
- Used for complex journal entries

## Audit Pattern
All tables follow consistent audit pattern:
- CreatedBy, CreatedDate
- ModifiedBy, ModifiedDate
- DeletedBy, DeletedDate (Soft Delete)

