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
                            .ThenInclude(c => c.Breed) //  التأكد من جلب السلالة
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

            var recordsDto = recordsList.Select(d =>
            {
                var egyptDate = TimeZoneHelper.ToEgyptTime(d.Date);

                //  الحصول على نسبة النفوق المستهدف حسب السلالة وأسبوع اليوم
                var targetMortality = _context.TargetMortalitySettings
                    .Where(t => t.BreedId == d.Cycle.BreedId && //  استخدم الاسم من الـ navigation
                                t.WeekStart <= d.DayNumber &&
                                t.WeekEnd >= d.DayNumber)
                    .Select(t => t.ExpectedMortalityRate)
                    .FirstOrDefault(); // 0 لو مفيش إعداد

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

                    // النفوق الفعلي
                    MortalityRate = d.Cycle.ChickCount == 0
                        ? 0
                        : Math.Round((decimal)d.DeadCumulative / d.Cycle.ChickCount * 100, 2),

                    //  الإضافة: النسبة المستهدفة
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

    // ===== STEP 1: جلب المستهدف من إعدادات السلالة =====
    var targetPerBirdGram = await _context.FeedConsumptionSettings
        .Where(t => t.BreedId == cycle.BreedId &&
                    t.WeekStart <= record.DayNumber &&
                    t.WeekEnd >= record.DayNumber)
        .Select(t => t.TargetPerBirdGram)
        .FirstOrDefaultAsync();

    if (targetPerBirdGram == 0)
        return BadRequest("لا يوجد مستهدف للعلف لهذا اليوم للسلالة المحددة.");

    // ===== STEP 2: معالجة FeedConsumptions =====
    decimal totalFeedKg = 0; // بالكيلو
    foreach (var feedDto in dto.FeedConsumptions)
    {
        var warehouseItem = await _context.WarehouseItems
            .Include(w => w.Item)
            .FirstOrDefaultAsync(w => w.ItemId == feedDto.ItemId && w.WarehouseId == dto.WarehouseId);

        if (warehouseItem == null)
            return BadRequest($"Item {feedDto.ItemId} not found in this warehouse.");

        // تحويل الكمية من طن → كيلو
        //var feedKg = feedDto.Quantity * 1000m;
        var feedKg = feedDto.Quantity;


        if (warehouseItem.Quantity < feedKg)
            return BadRequest($"Not enough stock for item {warehouseItem.Item.Name}");

        warehouseItem.Quantity -= feedKg;
        warehouseItem.Withdrawn += feedKg;

        record.FeedConsumptions.Add(new DailyFeedConsumption
        {
            ItemId = feedDto.ItemId,
            Quantity = feedKg,
            Cost = feedKg * warehouseItem.PricePerUnit
        });

        totalFeedKg += feedKg;
    }

    // ===== STEP 3: معالجة MedicineConsumptions =====
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

    // ===== STEP 4: حساب الاستهلاك اليومي والتراكمي =====
    var feedPerBirdGram = remainingChicks == 0 ? 0 : totalFeedKg * 1000 / remainingChicks;

    // التراكمي للطائر (كجم)
    decimal cumulativeFeedPerBirdKg = cycle.DailyRecords
        .Where(d => d.Id != record.Id)
        .Sum(d => d.FeedConsumptions.Sum(f => f.Quantity)) / (decimal)cycle.ChickCount;
    cumulativeFeedPerBirdKg += totalFeedKg / (decimal)cycle.ChickCount;

    // الاستهلاك اليومي للعنبر (طن)
    var feedPerHouseTon = totalFeedKg / 1000m;

    // التراكمي للعنبر (طن)
    decimal cumulativeFeedHouseTon = cycle.DailyRecords
        .Where(d => d.Id != record.Id)
        .Sum(d => d.FeedConsumptions.Sum(f => f.Quantity)) / 1000m;
    cumulativeFeedHouseTon += feedPerHouseTon;

    // نسبة التحقيق
    var achievementPercent = targetPerBirdGram == 0 ? 0 : (feedPerBirdGram / targetPerBirdGram) * 100;

    string feedStatus;
    if (achievementPercent > 102) feedStatus = "هدر 🔴";
    else if (achievementPercent < 95) feedStatus = "ناقص ⚠️";
    else feedStatus = "طبيعي ✅";

    _context.DailyRecords.Add(record);
    await _context.SaveChangesAsync();

    var egyptDate = TimeZoneHelper.ToEgyptTime(record.Date);

    // ===== STEP 5: رجّع كل البيانات في DTO =====
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
        MortalityRate = cycle.ChickCount == 0 ? 0 : Math.Round((decimal)record.DeadCumulative / cycle.ChickCount * 100, 2),

        FeedPerBirdGram = feedPerBirdGram,
        CumulativeFeedPerBirdKg = cumulativeFeedPerBirdKg,
        FeedPerHouseTon = feedPerHouseTon,
        CumulativeFeedHouseTon = cumulativeFeedHouseTon,
        FeedTargetPerBirdGram = targetPerBirdGram,
        FeedAchievementPercent = achievementPercent,
        FeedStatus = feedStatus,

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