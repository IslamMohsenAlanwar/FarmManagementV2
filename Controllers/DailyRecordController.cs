using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DailyRecordsController : ControllerBase
    {
        private readonly FarmDbContext _context;
        public DailyRecordsController(FarmDbContext context) => _context = context;

        // ================= GET =================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DailyRecordDto>>> GetDailyRecords([FromQuery] int? cycleId)
        {
            var query = _context.DailyRecords
                        .Include(d => d.FeedConsumptions).ThenInclude(f => f.Item)
                        .Include(d => d.MedicineConsumptions).ThenInclude(m => m.Item)
                        .AsQueryable();

            if (cycleId.HasValue)
                query = query.Where(d => d.CycleId == cycleId.Value);

            var records = await query.OrderBy(d => d.Date).ToListAsync();

            return records.Select(d =>
            {
                var egyptDate = TimeZoneHelper.ToEgyptTime(d.Date);

                return new DailyRecordDto
                {
                    Id = d.Id,
                    CycleId = d.CycleId,
                    Date = egyptDate,
                    DayName = TimeZoneHelper.GetArabicDayName(egyptDate),
                    DayNumber = d.DayNumber,
                    ChickAge = d.ChickAge,
                    DeadCount = d.DeadCount,
                    DeadCumulative = d.DeadCumulative,
                    RemainingChicks = d.RemainingChicks,
                    FeedConsumptions = d.FeedConsumptions.Select(f => new FeedConsumptionDto
                    {
                        ItemId = f.ItemId,
                        ItemName = f.Item?.Name ?? "غير محدد",
                        Quantity = f.Quantity,
                        Cost = f.Cost
                    }).ToList(),
                    MedicineConsumptions = d.MedicineConsumptions.Select(m => new MedicineConsumptionDto
                    {
                        ItemId = m.ItemId,
                        ItemName = m.Item?.Name ?? "غير محدد",
                        Quantity = m.Quantity,
                        Cost = m.Cost
                    }).ToList()
                };
            }).ToList();
        }

        // ================= POST =================
        [HttpPost]
        public async Task<ActionResult<DailyRecordDto>> CreateDailyRecord(DailyRecordCreateDto dto)
        {
            // جلب الدورة
            var cycle = await _context.Cycles
                .Include(c => c.DailyRecords)
                    .ThenInclude(d => d.FeedConsumptions)
                .Include(c => c.DailyRecords)
                    .ThenInclude(d => d.MedicineConsumptions)
                .FirstOrDefaultAsync(c => c.Id == dto.CycleId);

            if (cycle == null) return BadRequest("Cycle not found.");

            var lastRecord = cycle.DailyRecords.OrderByDescending(d => d.Date).FirstOrDefault();
            var recordDate = lastRecord != null ? lastRecord.Date.AddDays(1) : DateTime.UtcNow.Date;

            int previousDeadCumulative = lastRecord?.DeadCumulative ?? 0;
            int remainingChicks = Math.Max(0, cycle.ChickCount - (previousDeadCumulative + dto.DeadCount));

            var record = new DailyRecord
            {
                CycleId = dto.CycleId,
                Date = DateTime.SpecifyKind(recordDate, DateTimeKind.Utc),
                DayNumber = lastRecord != null ? lastRecord.DayNumber + 1 : 1,
                ChickAge = cycle.ChickAge + (lastRecord?.DayNumber ?? 0),
                DeadCount = dto.DeadCount,
                DeadCumulative = previousDeadCumulative + dto.DeadCount,
                RemainingChicks = remainingChicks,
                FeedConsumptions = new List<DailyFeedConsumption>(),
                MedicineConsumptions = new List<DailyMedicineConsumption>()
            };

            // ================= معالجة FeedConsumptions =================
            foreach (var feedDto in dto.FeedConsumptions)
            {
                var warehouseItem = await _context.WarehouseItems
                    .Include(w => w.Item)
                    .FirstOrDefaultAsync(w => w.ItemId == feedDto.ItemId && w.WarehouseId == dto.WarehouseId);

                if (warehouseItem == null)
                    return BadRequest($"Item {feedDto.ItemId} not found in this warehouse.");

                if (warehouseItem.Quantity < feedDto.Quantity)
                    return BadRequest($"Not enough stock for item {warehouseItem.Item.Name}");

                // خصم الكمية من المخزن
                warehouseItem.Quantity -= feedDto.Quantity;
                warehouseItem.Withdrawn += feedDto.Quantity;

                // إضافة الاستهلاك اليومي
                record.FeedConsumptions.Add(new DailyFeedConsumption
                {
                    ItemId = feedDto.ItemId,
                    Quantity = feedDto.Quantity,
                    Cost = feedDto.Quantity * warehouseItem.PricePerUnit
                });
            }

            // ================= معالجة MedicineConsumptions =================
            foreach (var medDto in dto.MedicineConsumptions)
            {
                var warehouseItem = await _context.WarehouseItems
                    .Include(w => w.Item)
                    .FirstOrDefaultAsync(w => w.ItemId == medDto.ItemId && w.WarehouseId == dto.WarehouseId);

                if (warehouseItem == null)
                    return BadRequest($"Item {medDto.ItemId} not found in this warehouse.");

                if (warehouseItem.Quantity < medDto.Quantity)
                    return BadRequest($"Not enough stock for item {warehouseItem.Item.Name}");

                warehouseItem.Quantity -= medDto.Quantity;
                warehouseItem.Withdrawn += medDto.Quantity;

                record.MedicineConsumptions.Add(new DailyMedicineConsumption
                {
                    ItemId = medDto.ItemId,
                    Quantity = medDto.Quantity,
                    Cost = medDto.Quantity * warehouseItem.PricePerUnit
                });
            }

            _context.DailyRecords.Add(record);
            await _context.SaveChangesAsync();

            var egyptDate = TimeZoneHelper.ToEgyptTime(record.Date);

            return new DailyRecordDto
            {
                Id = record.Id,
                CycleId = record.CycleId,
                Date = egyptDate,
                DayName = TimeZoneHelper.GetArabicDayName(egyptDate),
                DayNumber = record.DayNumber,
                ChickAge = record.ChickAge,
                DeadCount = record.DeadCount,
                DeadCumulative = record.DeadCumulative,
                RemainingChicks = record.RemainingChicks,
                FeedConsumptions = record.FeedConsumptions.Select(f => new FeedConsumptionDto
                {
                    ItemId = f.ItemId,
                    ItemName = f.Item?.Name ?? "غير محدد",
                    Quantity = f.Quantity,
                    Cost = f.Cost
                }).ToList(),
                MedicineConsumptions = record.MedicineConsumptions.Select(m => new MedicineConsumptionDto
                {
                    ItemId = m.ItemId,
                    ItemName = m.Item?.Name ?? "غير محدد",
                    Quantity = m.Quantity,
                    Cost = m.Cost
                }).ToList()
            };
        }
    }
}
