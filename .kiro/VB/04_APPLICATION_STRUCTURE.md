# APPLICATION STRUCTURE

## Module Organization

### 1. Applications/Dashboards/
**Main UI & Navigation**
- LOGIN.vb - User authentication
- MENUUTAMA.vb - Main menu/dashboard
- MetroMenu.vb, MetroUi.vb - Metro-style UI
- About.vb - About dialog
- Screens/ - Screen modules (Jurnal, Master, Report, Setting, Transaction)

### 2. Applications/Masters/
**Master Data Management**
- Cars/ - Vehicle management
- Customers/ - Customer/Pangkalan management
- DeliveryPoints/ - Delivery point management
- Drivers/ - Driver management
- MasterBarangs/ - Product/LPG management
- Penggunas/ - User management
- Profiles/ - Company/PT management
- Suppliers/ - Supplier management
- SupplyPoints/ - Supply point management
- Usahas/ - Business type management

### 3. Applications/Transactions/
**Transaction Processing**
- Pembelian/ - Purchase transactions
  - Kidungs/AddBeliKidung.vb - Add purchase
  - Kidungs/ListBeliKidung.vb - List purchases
- Penjualan/ - Sales transactions
  - Kidungs/AddJualKidung.vb - Add sales
  - Kidungs/ListJualKidung.vb - List sales
- Piutang/ - Accounts receivable
- Utang/ - Accounts payable
- ReturBeli/ - Purchase returns
- ReturJual/ - Sales returns
- PolaSewa/ - Rental patterns


### 4. Applications/Akuntings/
**Accounting Module**
- COAs/ - Chart of Accounts
- Kass/ - Cash management
- KelompokAkuns/ - Account groups
- LabaRugi/ - Profit & Loss
- Neracas/ - Balance Sheet
- TransaksionKas/ - Cash transactions

### 5. Applications/LabaRugis/
**Profit/Loss by Company**
- KIDUNGS/ - Kidung P&L
- MINTANGASINDOS/ - Mintan Gasindo P&L
- MINTANSAMUDRAS/ - Mintan Samudra P&L
- SUDIMULYOS/ - Sudi Mulyo P&L

### 6. Applications/Exceptios/
**Exception Handling**
- CloseExceptions.vb - Exception closing logic

### 7. Report/
**Reporting Module**
- CR/ - Crystal Reports
  - COA/, LabaRugi/, Masters/, Transaction/
- DataSheet/ - Report data sources
  - COA/, LabaRugi/, Masters/, Transaction/
- Master/ - Master report templates

## Core Modules (Root Level)

### Koneksi.vb
**Database Connection Module**
- Konek() - Opens SQL Server connection
- HashPassword() - PBKDF2 password hashing
- VerifyPassword() - Password verification
- Global variables: CONN, CMD, DR, DA, DS, STR


### ConstConfig.vb
**Configuration Constants**
- Company codes: kidungPT, mintanSamudraPT, etc.
- Payment types: typeBayar, typeTunai, typeKredit
- FTP settings: ftpServerFaktur, ftpServerInvoice
- Pricing: getHargaJual, getHargaBeli
- Format settings: formatDate, sizeFont

### ConstSql.vb
**SQL Query Constants**
- Transaction queries: getFakturKidung, getListBeliKidung
- Detail queries: getDetailBeliKidung, getTransaksiKidung
- Stock history: getHistoryStockKidung
- Path settings: pathInvoice, pathFaktur

### ConstSqlMaster.vb
**Master Data Query Constants**
- getCar, getCarByPt
- getProfile, getProfileByName
- getCustomer, getCustomerByPt
- getDriver, getDriverByPt
- getBarang, getDetailBarang
- getSupplier, getSuplyPoint
- getPengguna, getUsaha

### ConfigSqlAkun.vb
**Accounting Query Constants**
- getCOA, getCOAById, getCOAByPt
- getKasMasterById, getKasById
- getKelompokAkun
- getKasTransaksiByPTByCoa
- getTempTransaksiByPt

