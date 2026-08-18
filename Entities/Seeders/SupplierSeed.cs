using AccuFlow.Domain.Entities;

namespace AccuFlow.Entities.Seeders
{
    public static class SupplierSeed
    {
        public static List<SupplierEntity> GetSupplierSeedData()
        {
            return new List<SupplierEntity>
            {
                // Individual Suppliers
                new SupplierEntity
                {
                    SupplierId = Guid.Parse("D0000000-0000-0000-0000-000000000001"),
                    SupplierCode = "SUPP-00001",
                    SupplierName = "John Doe",
                    SupplierType = "Individual",
                    ContactPerson = "John Doe",
                    Phone = "+62 812-3456-7890",
                    Email = "john.doe@email.com",
                    Address = "Jl. Sudirman No. 123",
                    City = "Jakarta",
                    State = "DKI Jakarta",
                    PostalCode = "12190",
                    Country = "Indonesia",
                    CreditLimit = 10000000,
                    PaymentTerms = 30,
                    CurrentBalance = 0,
                    TaxId = "12.345.678.9-012.000",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new SupplierEntity
                {
                    SupplierId = Guid.Parse("D0000000-0000-0000-0000-000000000002"),
                    SupplierCode = "SUPP-00002",
                    SupplierName = "Jane Smith",
                    SupplierType = "Individual",
                    ContactPerson = "Jane Smith",
                    Phone = "+62 813-9876-5432",
                    Email = "jane.smith@email.com",
                    Address = "Jl. Gatot Subroto No. 456",
                    City = "Bandung",
                    State = "Jawa Barat",
                    PostalCode = "40123",
                    Country = "Indonesia",
                    CreditLimit = 5000000,
                    PaymentTerms = 30,
                    CurrentBalance = 0,
                    TaxId = "98.765.432.1-098.000",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                
                // Corporate Suppliers
                new SupplierEntity
                {
                    SupplierId = Guid.Parse("D0000000-0000-0000-0000-000000000003"),
                    SupplierCode = "SUPP-00003",
                    SupplierName = "PT Maju Jaya Abadi",
                    SupplierType = "Corporate",
                    ContactPerson = "Budi Santoso",
                    Phone = "+62 21-5555-1234",
                    Email = "info@majujaya.co.id",
                    Website = "www.majujaya.co.id",
                    Address = "Jl. HR Rasuna Said Kav. 1-2",
                    City = "Jakarta",
                    State = "DKI Jakarta",
                    PostalCode = "12950",
                    Country = "Indonesia",
                    CreditLimit = 50000000,
                    PaymentTerms = 45,
                    CurrentBalance = 0,
                    TaxId = "01.234.567.8-901.000",
                    Notes = "Corporate supplier with good payment history",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new SupplierEntity
                {
                    SupplierId = Guid.Parse("D0000000-0000-0000-0000-000000000004"),
                    SupplierCode = "SUPP-00004",
                    SupplierName = "CV Berkah Sejahtera",
                    SupplierType = "Corporate",
                    ContactPerson = "Siti Nurhaliza",
                    Phone = "+62 22-7777-8888",
                    Email = "contact@berkahsejahtera.com",
                    Website = "www.berkahsejahtera.com",
                    Address = "Jl. Asia Afrika No. 789",
                    City = "Bandung",
                    State = "Jawa Barat",
                    PostalCode = "40111",
                    Country = "Indonesia",
                    CreditLimit = 25000000,
                    PaymentTerms = 30,
                    CurrentBalance = 0,
                    TaxId = "11.222.333.4-555.000",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new SupplierEntity
                {
                    SupplierId = Guid.Parse("D0000000-0000-0000-0000-000000000005"),
                    SupplierCode = "SUPP-00005",
                    SupplierName = "PT Teknologi Nusantara",
                    SupplierType = "Corporate",
                    ContactPerson = "Ahmad Hidayat",
                    Phone = "+62 31-9999-0000",
                    Email = "sales@teknologi-nusantara.id",
                    Website = "www.teknologi-nusantara.id",
                    Address = "Jl. Raya Darmo No. 100",
                    City = "Surabaya",
                    State = "Jawa Timur",
                    PostalCode = "60264",
                    Country = "Indonesia",
                    CreditLimit = 75000000,
                    PaymentTerms = 60,
                    CurrentBalance = 0,
                    TaxId = "22.333.444.5-666.000",
                    Notes = "Large corporate supplier - VIP",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                
                // Government Supplier
                new SupplierEntity
                {
                    SupplierId = Guid.Parse("D0000000-0000-0000-0000-000000000006"),
                    SupplierCode = "SUPP-00006",
                    SupplierName = "Dinas Pendidikan Provinsi DKI Jakarta",
                    SupplierType = "Government",
                    ContactPerson = "Dr. Bambang Suryadi",
                    Phone = "+62 21-3456-7890",
                    Email = "disdik@jakarta.go.id",
                    Website = "disdik.jakarta.go.id",
                    Address = "Jl. Gatot Subroto Kav. 40-41",
                    City = "Jakarta",
                    State = "DKI Jakarta",
                    PostalCode = "12190",
                    Country = "Indonesia",
                    CreditLimit = 100000000,
                    PaymentTerms = 90,
                    CurrentBalance = 0,
                    TaxId = "00.111.222.3-444.000",
                    Notes = "Government institution - requires special documentation",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new SupplierEntity
                {
                    SupplierId = Guid.Parse("D0000000-0000-0000-0000-000000000007"),
                    SupplierCode = "SUPP-00007",
                    SupplierName = "Kementerian Kesehatan RI",
                    SupplierType = "Government",
                    ContactPerson = "Prof. Dr. Siti Rahayu",
                    Phone = "+62 21-5200-1234",
                    Email = "info@kemkes.go.id",
                    Website = "www.kemkes.go.id",
                    Address = "Jl. HR Rasuna Said Blok X5 Kav. 4-9",
                    City = "Jakarta",
                    State = "DKI Jakarta",
                    PostalCode = "12950",
                    Country = "Indonesia",
                    CreditLimit = 200000000,
                    PaymentTerms = 90,
                    CurrentBalance = 0,
                    TaxId = "00.999.888.7-666.000",
                    Notes = "Ministry - requires tender process",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };
        }
    }
}

