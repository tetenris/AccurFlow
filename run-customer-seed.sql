-- Insert Customer Seed Data
-- Run this script to manually seed customer data

INSERT INTO [Customers] 
([CustomerId], [CustomerCode], [CustomerName], [CustomerType], [ContactPerson], [Phone], [Email], [Address], [City], [State], [PostalCode], [Country], [CreditLimit], [PaymentTerms], [CurrentBalance], [TaxId], [IsActive], [CreatedBy], [CreatedAt], [IsDeleted])
VALUES
-- Individual Customers
('C0000000-0000-0000-0000-000000000001', 'CUST-00001', 'John Doe', 'Individual', 'John Doe', '+62 812-3456-7890', 'john.doe@email.com', 'Jl. Sudirman No. 123', 'Jakarta', 'DKI Jakarta', '12190', 'Indonesia', 10000000, 30, 0, '12.345.678.9-012.000', 1, 'system', GETUTCDATE(), 0),
('C0000000-0000-0000-0000-000000000002', 'CUST-00002', 'Jane Smith', 'Individual', 'Jane Smith', '+62 813-9876-5432', 'jane.smith@email.com', 'Jl. Gatot Subroto No. 456', 'Bandung', 'Jawa Barat', '40123', 'Indonesia', 5000000, 30, 0, '98.765.432.1-098.000', 1, 'system', GETUTCDATE(), 0),

-- Corporate Customers
('C0000000-0000-0000-0000-000000000003', 'CUST-00003', 'PT Maju Jaya Abadi', 'Corporate', 'Budi Santoso', '+62 21-5555-1234', 'info@majujaya.co.id', 'Jl. HR Rasuna Said Kav. 1-2', 'Jakarta', 'DKI Jakarta', '12950', 'Indonesia', 50000000, 45, 0, '01.234.567.8-901.000', 1, 'system', GETUTCDATE(), 0),
('C0000000-0000-0000-0000-000000000004', 'CUST-00004', 'CV Berkah Sejahtera', 'Corporate', 'Siti Nurhaliza', '+62 22-7777-8888', 'contact@berkahsejahtera.com', 'Jl. Asia Afrika No. 789', 'Bandung', 'Jawa Barat', '40111', 'Indonesia', 25000000, 30, 0, '11.222.333.4-555.000', 1, 'system', GETUTCDATE(), 0),
('C0000000-0000-0000-0000-000000000005', 'CUST-00005', 'PT Teknologi Nusantara', 'Corporate', 'Ahmad Hidayat', '+62 31-9999-0000', 'sales@teknologi-nusantara.id', 'Jl. Raya Darmo No. 100', 'Surabaya', 'Jawa Timur', '60264', 'Indonesia', 75000000, 60, 0, '22.333.444.5-666.000', 1, 'system', GETUTCDATE(), 0),

-- Government Customer
('C0000000-0000-0000-0000-000000000006', 'CUST-00006', 'Dinas Pendidikan Provinsi DKI Jakarta', 'Government', 'Dr. Bambang Suryadi', '+62 21-3456-7890', 'disdik@jakarta.go.id', 'Jl. Gatot Subroto Kav. 40-41', 'Jakarta', 'DKI Jakarta', '12190', 'Indonesia', 100000000, 90, 0, '00.111.222.3-444.000', 1, 'system', GETUTCDATE(), 0),
('C0000000-0000-0000-0000-000000000007', 'CUST-00007', 'Kementerian Kesehatan RI', 'Government', 'Prof. Dr. Siti Rahayu', '+62 21-5200-1234', 'info@kemkes.go.id', 'Jl. HR Rasuna Said Blok X5 Kav. 4-9', 'Jakarta', 'DKI Jakarta', '12950', 'Indonesia', 200000000, 90, 0, '00.999.888.7-666.000', 1, 'system', GETUTCDATE(), 0);

-- Check inserted data
SELECT COUNT(*) as TotalCustomers FROM [Customers] WHERE IsDeleted = 0;
SELECT * FROM [Customers] WHERE IsDeleted = 0 ORDER BY CustomerCode;
