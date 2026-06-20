using AccuFlow.Entities.Entity;

namespace AccuFlow.Entities.Seeders
{
    public static class BusinessModuleSeed
    {
        public static List<WarehouseEntity> GetWarehouseSeedData()
        {
            return new List<WarehouseEntity>
            {
                new WarehouseEntity
                {
                    WarehouseId = Guid.Parse("E0000000-0000-0000-0000-000000000001"),
                    WarehouseCode = "WH-00001",
                    WarehouseName = "Main Warehouse",
                    Address = "Jakarta",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };
        }

        public static List<TaxEntity> GetTaxSeedData()
        {
            return new List<TaxEntity>
            {
                new TaxEntity
                {
                    TaxId = Guid.Parse("F0000000-0000-0000-0000-000000000001"),
                    TaxCode = "PPN11",
                    TaxName = "PPN 11%",
                    Rate = 11,
                    TaxType = "VAT",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };
        }
    }
}
