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

        public static List<ItemGroupEntity> GetItemGroupSeedData()
        {
            return new List<ItemGroupEntity>
            {
                new ItemGroupEntity
                {
                    ItemGroupId = Guid.Parse("E0000000-0000-0000-0000-000000000010"),
                    GroupCode = "GRP-0001",
                    GroupName = "General",
                    Description = "Default item group",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ItemGroupEntity
                {
                    ItemGroupId = Guid.Parse("E0000000-0000-0000-0000-000000000011"),
                    GroupCode = "GRP-0002",
                    GroupName = "Raw Material",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ItemGroupEntity
                {
                    ItemGroupId = Guid.Parse("E0000000-0000-0000-0000-000000000012"),
                    GroupCode = "GRP-0003",
                    GroupName = "Finished Goods",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new ItemGroupEntity
                {
                    ItemGroupId = Guid.Parse("E0000000-0000-0000-0000-000000000013"),
                    GroupCode = "GRP-0004",
                    GroupName = "Spare Part",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };
        }

        public static List<UnitEntity> GetUnitSeedData()
        {
            return new List<UnitEntity>
            {
                BuildUnit("E0000000-0000-0000-0000-000000000020", "PCS", "Piece"),
                BuildUnit("E0000000-0000-0000-0000-000000000021", "BOX", "Box"),
                BuildUnit("E0000000-0000-0000-0000-000000000022", "SET", "Set"),
                BuildUnit("E0000000-0000-0000-0000-000000000023", "KG", "Kilogram"),
                BuildUnit("E0000000-0000-0000-0000-000000000024", "L", "Liter")
            };
        }

        private static UnitEntity BuildUnit(string id, string code, string name)
        {
            return new UnitEntity
            {
                UnitId = Guid.Parse(id),
                UnitCode = code,
                UnitName = name,
                IsActive = true,
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
