using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;
using System.Globalization;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EggProductionController : ControllerBase
    {
        private readonly FarmDbContext _context;
        public EggProductionController(FarmDbContext context) => _context = context;

        // ================= POST: EggProductionRecord =================
        [HttpPost]
        public async Task<ActionResult<EggProductionRecordDto>> Create(CreateEggProductionDto dto)
        {
            const int EggsPerCarton = 30;

            var cycle = await _context.Cycles
                .Include(c => c.DailyRecords)
                .FirstOrDefaultAsync(c => c.Id == dto.CycleId);

            if (cycle == null) return BadRequest("Cycle not found.");

            var lastDaily = cycle.DailyRecords
                                .OrderByDescending(d => d.Date)
                                .FirstOrDefault();

            if (lastDaily == null) return BadRequest("No daily record for this cycle.");

            var liveBirds = lastDaily.RemainingChicks;
            var totalEggs = dto.CartonsCount * EggsPerCarton;
            var productionRate = liveBirds == 0 ? 0 : (decimal)totalEggs / liveBirds * 100;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var record = new EggProductionRecord
                {
                    FarmId = dto.FarmId,
                    BarnId = dto.BarnId,
                    CycleId = dto.CycleId,
                    Date = dto.Date,
                    CartonsCount = dto.CartonsCount,
                    TotalEggs = totalEggs,
                    LiveBirdsCount = liveBirds,
                    ProductionRate = productionRate,
                    Notes = dto.Notes
                };

                _context.EggProductionRecords.Add(record);
                await _context.SaveChangesAsync();

               
                var eggItem = await _context.Items
                    .Where(i => i.ItemType == ItemType.Egg)
                    .OrderBy(i => i.Id)
                    .FirstOrDefaultAsync();

                var eggWarehouse = await _context.Warehouses
                    .Where(w => w.FarmId == dto.FarmId)
                    .OrderBy(w => w.Id)
                    .FirstOrDefaultAsync();

                if (eggItem == null || eggWarehouse == null)
                    return BadRequest("Egg item or warehouse not configured.");

                var warehouseItem = await _context.WarehouseItems
                    .Where(wi => wi.WarehouseId == eggWarehouse.Id && wi.ItemId == eggItem.Id)
                    .OrderBy(wi => wi.Id)
                    .FirstOrDefaultAsync();

                if (warehouseItem == null)
                {
                    warehouseItem = new WarehouseItem
                    {
                        WarehouseId = eggWarehouse.Id,
                        ItemId = eggItem.Id,
                        Quantity = 0,
                        Type = "Egg"
                    };
                    _context.WarehouseItems.Add(warehouseItem);
                }

                warehouseItem.Quantity += dto.CartonsCount;

                var warehouseTransaction = new WarehouseTransaction
                {
                    WarehouseId = eggWarehouse.Id,
                    ItemId = eggItem.Id,
                    Quantity = dto.CartonsCount,
                    Date = dto.Date,
                    TransactionType = "EggProduction",
                    EggProductionRecordId = record.Id
                };
                _context.WarehouseTransactions.Add(warehouseTransaction);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new EggProductionRecordDto
                {
                    Id = record.Id,
                    FarmId = record.FarmId,
                    BarnId = record.BarnId,
                    CycleId = record.CycleId,
                    Date = record.Date,
                    CartonsCount = record.CartonsCount,
                    TotalEggs = record.TotalEggs,
                    LiveBirdsCount = record.LiveBirdsCount,
                    ProductionRate = record.ProductionRate,
                    Notes = record.Notes
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ================= GET: AllEggFromWarehouse =================
        [HttpGet("warehouse")]
        public async Task<ActionResult<IEnumerable<WarehouseItemDto>>> GetAllEggsInWarehouse()
        {
            var eggs = await _context.WarehouseItems
                                     .Include(w => w.Item)
                                     .Include(w => w.Warehouse)
                                     .Where(w => w.Item.ItemType == ItemType.Egg)
                                     .ToListAsync();

            return eggs.Select(e => new WarehouseItemDto
            {
                WarehouseId = e.WarehouseId,
                WarehouseName = e.Warehouse.Name,
                ItemId = e.ItemId,
                ItemName = e.Item.Name,
                Quantity = e.Quantity
            }).ToList();
        }

        // ================= GET: EggProduction By Barn=================
        [HttpGet("farm/{farmId}/eggs")]
        public async Task<ActionResult<IEnumerable<EggProductionByBarnDto>>> GetEggProductionByFarm(int farmId)
        {
            var arabicCulture = new CultureInfo("ar-EG"); 

            // Egg Production Records
            var records = await _context.EggProductionRecords
                .Include(r => r.Barn)    
                .Where(r => r.FarmId == farmId)
                .OrderBy(r => r.Date)
                .ToListAsync();

            var eggItem = await _context.Items
                .Where(i => i.ItemType == ItemType.Egg)
                .OrderBy(i => i.Id)
                .FirstOrDefaultAsync();

            if (eggItem == null)
                return BadRequest("Egg item not configured.");

            var result = records.Select(r => new EggProductionByBarnDto
            {
                BarnName = r.Barn.Name,
                ItemName = eggItem.Name,
                Quantity = r.TotalEggs,   
                Day = arabicCulture.DateTimeFormat.GetDayName(r.Date.DayOfWeek), 
                Date = r.Date
            }).ToList();

            return result;
        }
    }
}
