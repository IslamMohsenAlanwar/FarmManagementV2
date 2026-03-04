using FarmManagement.API.Data;
using FarmManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace FarmManagement.API
{
    public static class SeedData
    {
        public static async Task Initialize(FarmDbContext context)
        {
            // ===== Apply Migrations =====
            await context.Database.MigrateAsync();

            // =====================================================
            // ===================== OWNER =========================
            // =====================================================
            if (!await context.AppUsers.AnyAsync(u => u.UserType == UserType.Owner))
            {
                var owner = new AppUser
                {
                    Username = "owner",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                    UserType = UserType.Owner
                };

                context.AppUsers.Add(owner);
                await context.SaveChangesAsync();
            }

            // =====================================================
            // ===================== FARMS =========================
            // =====================================================
            if (!await context.Farms.AnyAsync())
            {
                var farm = new Farm
                {
                    Name = "مزرعة 1",
                    Description = "وصف مزرعة 1"
                };

                context.Farms.Add(farm);
                await context.SaveChangesAsync();
            }

            // =====================================================
            // ===================== TRADERS =======================
            // =====================================================
            /*
            if (!await context.Traders.AnyAsync())
            {
                var trader1 = new Trader { Name = "Trader 1", Mobile = "01000000001" };
                var trader2 = new Trader { Name = "Trader 2", Mobile = "01000000002" };

                context.Traders.AddRange(trader1, trader2);
                await context.SaveChangesAsync();
            }
            */

            // =====================================================
            // ===================== ITEMS =========================
            // =====================================================
            /*
            if (!await context.Items.AnyAsync())
            {
                var item1 = new Item
                {
                    Name = "بيض",
                    PricePerTon = 100,
                    ItemType = ItemType.Egg
                };

                context.Items.Add(item1);
                await context.SaveChangesAsync();
            }
            */

            // =====================================================
            // ===================== WAREHOUSES ====================
            // =====================================================
            /*
            if (!await context.Warehouses.AnyAsync())
            {
                var farm = await context.Farms.FirstAsync();

                var warehouse = new Warehouse
                {
                    Name = "مخزن مزرعة 1",
                    FarmId = farm.Id
                };

                context.Warehouses.Add(warehouse);
                await context.SaveChangesAsync();
            }
            */

            // =====================================================
            // ================== WAREHOUSE ITEMS ==================
            // =====================================================
            /*
            if (!await context.WarehouseItems.AnyAsync())
            {
                var warehouse = await context.Warehouses.FirstAsync();
                var items = await context.Items.ToListAsync();

                var warehouseItems = items.Select(i => new WarehouseItem
                {
                    WarehouseId = warehouse.Id,
                    ItemId = i.Id,
                    Quantity = 100,
                    PricePerUnit = i.PricePerTon,
                    Withdrawn = 0
                }).ToList();

                context.WarehouseItems.AddRange(warehouseItems);
                await context.SaveChangesAsync();
            }
            */
        }
    }
}