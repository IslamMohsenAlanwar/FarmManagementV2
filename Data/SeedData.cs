using FarmManagement.API.Data;
using FarmManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmManagement.API
{
    public static class SeedData
    {
        public static async Task Initialize(FarmDbContext context)
        {
            // ===== Farms =====
            if (!context.Farms.Any())
            {
                var farm = new Farm
                {
                    Name = "مزرعة 1",
                    Description = "وصف مزرعة 1"
                };

                context.Farms.Add(farm);
                await context.SaveChangesAsync();
            }

            // ===== Traders =====
            // if (!context.Traders.Any())
            // {
            //     var trader1 = new Trader { Name = "Trader 1", Mobile = "01000000001" };
            //     var trader2 = new Trader { Name = "Trader 2", Mobile = "01000000002" };
            //     context.Traders.AddRange(trader1, trader2);
            //     await context.SaveChangesAsync();
            // }

            // ===== Items =====
            // if (!context.Items.Any())
            // {
            //     var item1 = new Item { Name = "بيض", PricePerTon = 100 , ItemType= ItemType.Egg };
            //     context.Items.AddRange(item1);
            //     await context.SaveChangesAsync();
            // }

            // ===== Warehouses =====
            // if (!context.Warehouses.Any())
            // {
            //     var farm = await context.Farms.FirstAsync();

            //     var warehouse = new Warehouse
            //     {
            //         Name = "مخزن مزرعة 1",
            //         FarmId = farm.Id
            //     };
            //     context.Warehouses.Add(warehouse);
            //     await context.SaveChangesAsync();
            // }

            // ===== WarehouseItems =====
            // if (!context.WarehouseItems.Any())
            // {
            //     var warehouse = await context.Warehouses.FirstAsync();
            //     var items = await context.Items.ToListAsync();

            //     var warehouseItems = items.Select(i => new WarehouseItem
            //     {
            //         WarehouseId = warehouse.Id,
            //         ItemId = i.Id,
            //         Quantity = 100,
            //         PricePerUnit = i.PricePerTon,
            //         Withdrawn = 0
            //     }).ToList();

            //     context.WarehouseItems.AddRange(warehouseItems);
            //     await context.SaveChangesAsync();
            // }
        }
    }
}
