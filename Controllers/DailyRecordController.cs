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
        public async Task<ActionResult> GetDailyRecords(
            [FromQuery] int? cycleId,
            [FromQuery] int SkipCount = 0,
            [FromQuery] int MaxResultCount = 7)
        {
            var query = _context.DailyRecords
                        .Include(d => d.Cycle)
                            .ThenInclude(c => c.Breed)
                        .Include(d => d.FeedConsumptions).ThenInclude(f => f.Item)
                        .Include(d => d.MedicineConsumptions).ThenInclude(m => m.Item)
                        .AsQueryable();

            if (cycleId.HasValue)
                query = query.Where(d => d.CycleId == cycleId.Value);

            var totalCount = await query.CountAsync();

            var recordsList = await query
                .OrderByDescending(d => d.Id)
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .ToListAsync();

            // جلب كل إعدادات النفوق مرة واحدة
            var breedIds = recordsList.Select(r => r.Cycle.BreedId).Distinct().ToList();
            var targetSettings = await _context.TargetMortalitySettings
                .Where(t => breedIds.Contains(t.BreedId))
                .ToListAsync();

            var recordsDto = recordsList.Select(d =>
            {
                var egyptDate = TimeZoneHelper.ToEgyptTime(d.Date);

                var week = (d.ChickAge / 7) + 1;

                var targetMortality = targetSettings
                    .Where(t => t.BreedId == d.Cycle.BreedId &&
                                t.WeekStart <= week &&
                                t.WeekEnd >= week)
                    .Select(t => t.ExpectedMortalityRate)
                    .FirstOrDefault();

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
                    MortalityRate = d.Cycle.ChickCount == 0
                        ? 0
                        : Math.Round((decimal)d.DeadCumulative / d.Cycle.ChickCount * 100, 2),
                    TargetMortalityRate = targetMortality,
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

            return Ok(new
            {
                TotalCount = totalCount,
                DailyRecords = recordsDto
            });
        }

        // ================= POST =================
        [HttpPost]
        public async Task<ActionResult<DailyRecordDto>> CreateDailyRecord(DailyRecordCreateDto dto)
        {
            var cycle = await _context.Cycles
                .Include(c => c.DailyRecords)
                    .ThenInclude(d => d.FeedConsumptions)
                .Include(c => c.DailyRecords)
                    .ThenInclude(d => d.MedicineConsumptions)
                .FirstOrDefaultAsync(c => c.Id == dto.CycleId);

            if (cycle == null)
                return BadRequest("Cycle not found.");

            // آخر سجل (الأهم: حسب التاريخ مش DayNumber)
            var lastRecord = cycle.DailyRecords
                .OrderByDescending(d => d.Date)
                .FirstOrDefault();

            // بداية الدورة
            var baseDate = TimeZoneHelper.ToEgyptTime(cycle.StartDate).Date;

            DateTime recordDate;
            int dayNumber;
            int chickAge;

            if (lastRecord != null)
            {
                var lastDate = TimeZoneHelper.ToEgyptTime(lastRecord.Date).Date;
                recordDate = lastDate.AddDays(1);

                dayNumber = lastRecord.DayNumber + 1;
                chickAge = lastRecord.ChickAge + 1; // 👈 أهم تعديل هنا
            }
            else
            {
                recordDate = baseDate;
                dayNumber = 1;

                // 👇 يبدأ من عمر الدورة الأساسي
                chickAge = cycle.ChickAge;
            }

            int previousDeadCumulative = lastRecord?.DeadCumulative ?? 0;

            int remainingChicks = Math.Max(
                0,
                cycle.ChickCount - (previousDeadCumulative + dto.DeadCount)
            );

            var record = new DailyRecord
            {
                CycleId = dto.CycleId,
                Date = DateTime.SpecifyKind(recordDate, DateTimeKind.Utc),
                DayNumber = dayNumber,
                ChickAge = chickAge,

                DeadCount = dto.DeadCount,
                DeadCumulative = previousDeadCumulative + dto.DeadCount,
                RemainingChicks = remainingChicks,

                FeedConsumptions = new List<DailyFeedConsumption>(),
                MedicineConsumptions = new List<DailyMedicineConsumption>()
            };

            // الأسبوع
            var week = (chickAge / 7) + 1;

            var targetPerBirdGram = await _context.FeedConsumptionSettings
                .Where(t => t.BreedId == cycle.BreedId &&
                            t.WeekStart <= week &&
                            t.WeekEnd >= week)
                .Select(t => t.TargetPerBirdGram)
                .FirstOrDefaultAsync();

            if (targetPerBirdGram == 0)
                return BadRequest("لا يوجد مستهدف للعلف لهذا اليوم للسلالة المحددة.");

            decimal totalFeedKg = 0;

            foreach (var feedDto in dto.FeedConsumptions)
            {
                var warehouseItem = await _context.WarehouseItems
                    .Include(w => w.Item)
                    .FirstOrDefaultAsync(w =>
                        w.ItemId == feedDto.ItemId &&
                        w.WarehouseId == dto.WarehouseId);

                if (warehouseItem == null)
                    return BadRequest($"Item {feedDto.ItemId} not found in this warehouse.");

                if (warehouseItem.Quantity < feedDto.Quantity)
                    return BadRequest($"Not enough stock for item {warehouseItem.Item.Name}");

                warehouseItem.Quantity -= feedDto.Quantity;
                warehouseItem.Withdrawn += feedDto.Quantity;

                record.FeedConsumptions.Add(new DailyFeedConsumption
                {
                    ItemId = feedDto.ItemId,
                    Quantity = feedDto.Quantity,
                    Cost = feedDto.Quantity * warehouseItem.PricePerUnit
                });

                totalFeedKg += feedDto.Quantity;
            }

            foreach (var medDto in dto.MedicineConsumptions)
            {
                var warehouseItem = await _context.WarehouseItems
                    .Include(w => w.Item)
                    .FirstOrDefaultAsync(w =>
                        w.ItemId == medDto.ItemId &&
                        w.WarehouseId == dto.WarehouseId);

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

            var feedPerBirdGram = remainingChicks == 0
                ? 0
                : totalFeedKg * 1000 / remainingChicks;

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

                FeedPerBirdGram = feedPerBirdGram,

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