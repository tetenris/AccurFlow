# AccuFlow Project Generator

Generator untuk membuat project **AccuFlow** (Accounting System) berdasarkan template **KMSI.SuperApps.Ews**.

> Status: repository siap dipindahkan ke GitHub dan seluruh branch dapat dipush ke remote baru AccurFlow.

## 🚀 Cara Penggunaan

### 1. Generate Project Structure

Jalankan PowerShell script:

```powershell
.\GenerateAccuFlow.ps1
```

Script akan:
- ✅ Membuat folder structure lengkap
- ✅ Generate file `.csproj`
- ✅ Generate `appsettings.json`
- ✅ Generate `Program.cs`
- ✅ Copy Metronic template (wwwroot)

### 2. Buka Project

```powershell
cd D:\ASP.NET\2025\AKURAT\AccuFlow
code .  # Atau buka dengan Visual Studio
```

### 3. Restore Dependencies

```powershell
dotnet restore
```

### 4. Update Connection String

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=AccuFlowDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 5. Setup Database

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 6. Run Application

```powershell
dotnet run
```

Buka browser: `https://localhost:5001`

---

## 📁 Project Structure

```
AccuFlow/
├── Controllers/          # MVC Controllers
├── Models/              # View Models & DTOs
│   ├── Account/
│   ├── ChartOfAccount/
│   ├── JournalEntry/
│   ├── Invoice/
│   ├── PurchaseOrder/
│   ├── Inventory/
│   └── Report/
├── Services/            # Business Logic Services
├── Entities/            # Database Entities
│   ├── Context/
│   ├── Entity/
│   ├── EntityConfigurations/
│   └── Migrations/
├── Views/               # Razor Views
│   ├── Shared/
│   ├── Home/
│   ├── ChartOfAccount/
│   ├── JournalEntry/
│   ├── Invoice/
│   └── ...
├── wwwroot/             # Static Files (Metronic Template)
│   ├── assets/
│   ├── css/
│   ├── js/
│   └── theme/
├── Extentions/          # Extension Methods
├── Helpers/             # Helper Classes
├── Infrastructures/     # Infrastructure Services
└── Job/                 # Background Jobs (Hangfire)
```

---

## 🎯 Fitur Utama AccuFlow

### Accounting Core
- ✅ **Chart of Accounts** - Master akun
- ✅ **Journal Entry** - Pencatatan jurnal
- ✅ **General Ledger** - Buku besar
- ✅ **Trial Balance** - Neraca saldo

### Transaction Management
- ✅ **Invoice/Billing** - Faktur penjualan
- ✅ **Purchase Order** - Pembelian
- ✅ **Payment** - Pembayaran
- ✅ **Receipt** - Penerimaan

### Inventory
- ✅ **Stock Management** - Manajemen stok
- ✅ **Stock Opname** - Perhitungan stok
- ✅ **Stock Card** - Kartu stok

### Reports
- ✅ **Balance Sheet** - Neraca
- ✅ **Profit & Loss** - Laba Rugi
- ✅ **Cash Flow** - Arus Kas
- ✅ **Aging Report** - Umur piutang/hutang

---

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **UI Template**: Metronic (Bootstrap 5)
- **Background Jobs**: Hangfire
- **PDF Generation**: DinkToPdf
- **Excel**: NPOI

---

## 📝 Next Steps

Setelah project ter-generate:

1. **Customize Models** - Sesuaikan entity dengan kebutuhan
2. **Implement Services** - Buat business logic
3. **Create Views** - Design UI dengan Metronic
4. **Add Validations** - Tambahkan validasi data
5. **Setup Authentication** - Konfigurasi user & roles
6. **Add Reports** - Implementasi laporan keuangan

---

## 🤝 Support

Jika ada pertanyaan atau butuh bantuan, silakan hubungi developer.

---

**Happy Coding! 🚀**
