using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FarmManagement.API.Helpers;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EggProductionController : ControllerBase
    {
        private readonly FarmDbContext _context;
        public EggProductionController(FarmDbContext context) => _context = context;

        // ================= POST =================
        [HttpPost]
        public async Task<ActionResult<EggProductionRecordDto>> Create(CreateEggProductionDto dto)
        {
            const int EggsPerCarton = 30;

            var cycle = await _context.Cycles
                .Include(c => c.DailyRecords)
                .FirstOrDefaultAsync(c => c.Id == dto.CycleId);

            if (cycle == null) return BadRequest("Cycle not found.");

            var lastDaily = cycle.DailyRecords.OrderByDescending(d => d.Date).FirstOrDefault();
            if (lastDaily == null) return BadRequest("No daily record found.");

            var liveBirds = lastDaily.RemainingChicks;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var record = new EggProductionRecord
                {
                    FarmId = dto.FarmId,
                    BarnId = dto.BarnId,
                    CycleId = dto.CycleId,
                    Date = dto.Date,
                    LiveBirdsCount = liveBirds,
                    Notes = dto.Notes
                };

                int totalEggs = 0;

                foreach (var d in dto.Details)
                {
                    var eggs = d.CartonsCount * EggsPerCarton;
                    totalEggs += eggs;

                    record.Details.Add(new EggProductionDetail
                    {
                        EggQuality = d.EggQuality,
                        CartonsCount = d.CartonsCount,
                        TotalEggs = eggs
                    });
                }

                record.TotalEggs = totalEggs;
                record.ProductionRate = liveBirds == 0 ? 0 : (decimal)totalEggs / liveBirds * 100;

                _context.EggProductionRecords.Add(record);
                await _context.SaveChangesAsync();

                // ===== تحديث المخزن =====
                var eggItem = await _context.Items
                    .FirstOrDefaultAsync(i => i.ItemType == ItemType.Egg);

                var eggWarehouse = await _context.Warehouses
                    .FirstOrDefaultAsync(w => w.FarmId == dto.FarmId);

                if (eggItem == null || eggWarehouse == null)
                    return BadRequest("Egg item or warehouse not configured.");

                foreach (var d in dto.Details)
                {
                    var warehouseItem = await _context.WarehouseItems
                        .FirstOrDefaultAsync(w =>
                            w.WarehouseId == eggWarehouse.Id &&
                            w.ItemId == eggItem.Id &&
                            w.EggQuality == d.EggQuality);

                    if (warehouseItem == null)
                    {
                        warehouseItem = new WarehouseItem
                        {
                            WarehouseId = eggWarehouse.Id,
                            ItemId = eggItem.Id,
                            EggQuality = d.EggQuality,
                            Quantity = 0,
                            Type = "Egg"
                        };

                        _context.WarehouseItems.Add(warehouseItem);
                    }

                    warehouseItem.Quantity += d.CartonsCount;

                    _context.WarehouseTransactions.Add(new WarehouseTransaction
                    {
                        WarehouseId = eggWarehouse.Id,
                        ItemId = eggItem.Id,
                        Quantity = d.CartonsCount,
                        Date = dto.Date,
                        TransactionType = "EggProduction",
                        EggQuality = d.EggQuality,
                        EggProductionRecordId = record.Id
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new EggProductionRecordDto
                {
                    Id = record.Id,
                    Date = record.Date,
                    TotalEggs = record.TotalEggs,
                    LiveBirdsCount = record.LiveBirdsCount,
                    ProductionRate = record.ProductionRate,
                    Notes = record.Notes,
                    Details = record.Details.Select(d => new EggProductionDetailDto
                    {
                        EggQuality = d.EggQuality.ToArabic(),
                        CartonsCount = d.CartonsCount,
                        TotalEggs = d.TotalEggs
                    }).ToList()
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ================= GET: By Farm =================
        [HttpGet("farm/{farmId}/eggs")]
        public async Task<ActionResult<IEnumerable<EggProductionByBarnDto>>> GetEggProductionByFarm(int farmId)
        {
            var arabicCulture = new CultureInfo("ar-EG");

            var records = await _context.EggProductionRecords
                .Include(r => r.Barn)
                .Include(r => r.Details)
                .Where(r => r.FarmId == farmId)
                .OrderByDescending(r => r.Date) 
                .ToListAsync();

            var result = records.SelectMany(r => r.Details.Select(d => new EggProductionByBarnDto
            {
                BarnName = r.Barn.Name,
                EggQuality = d.EggQuality.ToArabic(),
                CartonsCount = d.CartonsCount,
                TotalEggs = d.TotalEggs,
                Day = arabicCulture.DateTimeFormat.GetDayName(r.Date.DayOfWeek),
                Date = r.Date
            })).ToList();

            return Ok(result);
        }




        [HttpGet("farm/{farmId}/warehouse-eggs")]
        // مخزن البيض
public async Task<ActionResult<IEnumerable<WarehouseItemDto>>> GetEggWarehouseItems(int farmId)
{
    var items = await _context.WarehouseItems
        .Include(wi => wi.Item)
        .Include(wi => wi.Warehouse)
        .Where(wi => wi.Warehouse.FarmId == farmId && wi.Item.ItemType == ItemType.Egg)
        .OrderByDescending(wi => wi.Id)
        .Select(wi => new WarehouseItemDto
        {
            Id = wi.Id,
            WarehouseId = wi.WarehouseId,
            WarehouseName = wi.Warehouse.Name,
            ItemId = wi.ItemId,
            ItemName = wi.Item.Name,
            Quantity = wi.Quantity,
            PricePerUnit = wi.PricePerUnit,
            Withdrawn = wi.Withdrawn,
            EggQuality = wi.EggQuality.HasValue ? wi.EggQuality.Value.ToArabic() : null
        })
        .ToListAsync();

    return Ok(items);
}
    }
}