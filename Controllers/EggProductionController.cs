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

            if (cycle == null)
                return BadRequest("Cycle not found.");

            // ================= VALIDATION (IMPORTANT) =================

            if (!cycle.DailyRecords.Any())
                return BadRequest("لا يوجد تسجيل يومي للدورة ، يجب إضافة تسجيل يومي أولاً.");

            var hasDailyRecordForDate = cycle.DailyRecords
                .Any(d => d.Date.Date == dto.Date.Date);

            if (!hasDailyRecordForDate)
                return BadRequest("يجب إنشاء تسجيل يومي أولاً لنفس تاريخ الإنتاج قبل تسجيل الإنتاج.");

            // ==========================================================

            var dailyRecord = cycle.DailyRecords
                .First(d => d.Date.Date == dto.Date.Date);

            var liveBirds = dailyRecord.RemainingChicks;

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

                decimal totalEggs = 0;

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
                record.ProductionRate = liveBirds == 0
                    ? 0
                    : (decimal)totalEggs / liveBirds * 100;

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
        public async Task<ActionResult> GetEggProductionByFarm(
            int farmId,
            int SkipCount = 0,
            int MaxResultCount = 7,
             int? CycleId = null)

        {
            var arabicCulture = new CultureInfo("ar-EG");

            var query = _context.EggProductionRecords
                .Include(r => r.Barn)
                .Include(r => r.Cycle)
                .Include(r => r.Details)
                .Where(r => r.FarmId == farmId);

            if (CycleId.HasValue)
            {
                query = query.Where(r => r.CycleId == CycleId.Value);
            }
            query = query.OrderByDescending(r => r.Date);


            var totalCount = await query.CountAsync();

            var records = await query
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .ToListAsync();

            // ================= ChickAge Lookup =================

            var cycleIds = records.Select(r => r.CycleId).Distinct().ToList();
            var dates = records.Select(r => r.Date.Date).Distinct().ToList();

            var dailyRecords = await _context.DailyRecords
                .Where(d => cycleIds.Contains(d.CycleId) && dates.Contains(d.Date.Date))
                .ToListAsync();

            var chickAgeLookup = dailyRecords
                .ToDictionary(
                    d => new { d.CycleId, Date = d.Date.Date },
                    d => d.ChickAge
                );

            // ================= Mapping =================

            var result = records.SelectMany(r => r.Details.Select(d => new EggProductionByBarnDto
            {
                BarnName = r.Barn.Name,
                CycleName = r.Cycle.Name,
                CycleId = r.CycleId,
                EggQuality = d.EggQuality.ToArabic(),
                CartonsCount = d.CartonsCount,
                TotalEggs = d.TotalEggs,
                Day = arabicCulture.DateTimeFormat.GetDayName(r.Date.DayOfWeek),
                Date = r.Date,

                //  Chick Age
                ChickAge = chickAgeLookup.TryGetValue(
                    new { r.CycleId, Date = r.Date.Date },
                    out var age
                ) ? age : 0
            })).ToList();

            return Ok(new
            {
                TotalCount = totalCount,
                EggRecords = result
            });
        }


        [HttpGet("farm/{farmId}/warehouse-eggs")]
        public async Task<ActionResult> GetEggWarehouseItems(
      int farmId,
      int SkipCount = 0,
      int MaxResultCount = 7)
        {
            var query = _context.WarehouseItems
                .Include(wi => wi.Item)
                .Include(wi => wi.Warehouse)
                .Where(wi => wi.Warehouse.FarmId == farmId
                          && wi.Item.ItemType == ItemType.Egg)
                .OrderByDescending(wi => wi.Id);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .Select(wi => new WarehouseItemDto
                {
                    Id = wi.Id,
                    WarehouseId = wi.WarehouseId,
                    WarehouseName = wi.Warehouse.Name,
                    ItemId = wi.ItemId,
                    ItemName = wi.Item.Name,

                    Quantity = wi.Quantity,
                    Withdrawn = wi.Withdrawn,

                    RemainingQuantity = wi.Quantity - wi.Withdrawn,

                    EggQuality = wi.EggQuality.HasValue
                        ? wi.EggQuality.Value.ToArabic()
                        : null
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                WarehouseEggs = items
            });
        }

        [HttpGet("farm/{farmId}/eggs/summary-by-cycle")]
        public async Task<ActionResult> GetEggSummaryByCycle(
            int farmId,
            int SkipCount = 0,
            int MaxResultCount = 7)
        {
            var baseQuery = _context.EggProductionRecords
                .Include(r => r.Cycle)
                .Include(r => r.Details)
                .Where(r => r.FarmId == farmId)
                .SelectMany(r => r.Details, (r, d) => new
                {
                    r.CycleId,
                    CycleName = r.Cycle.Name,
                    d.CartonsCount,
                    d.EggQuality
                })
                .GroupBy(x => new { x.CycleId, x.CycleName })
                .Select(g => new CycleEggSummaryDto
                {
                    CycleId = g.Key.CycleId,
                    CycleName = g.Key.CycleName,

                    TotalCartons = g.Sum(x => x.CartonsCount),

                    NormalEggs = g.Where(x => x.EggQuality == EggQualityType.Normal)
                                  .Sum(x => x.CartonsCount),

                    BrokenEggs = g.Where(x => x.EggQuality == EggQualityType.Broken)
                                  .Sum(x => x.CartonsCount),

                    DoubleEggs = g.Where(x => x.EggQuality == EggQualityType.Double)
                                  .Sum(x => x.CartonsCount),

                    FarzaEggs = g.Where(x => x.EggQuality == EggQualityType.Farza)
                                 .Sum(x => x.CartonsCount)
                });

            // 🔥 إجمالي عدد الدورات
            var totalCount = await baseQuery.CountAsync();

            // 🔥 Pagination على الدورات
            var result = await baseQuery
                .OrderByDescending(x => x.CycleId)
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Data = result
            });
        }
    }
}