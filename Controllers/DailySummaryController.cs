using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.DTOs;
using FarmManagement.API.Data;
using FarmManagement.API.Models;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DailySummaryController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public DailySummaryController(FarmDbContext context)
        {
            _context = context;
        }

        // ================= GET Daily Summary with Pagination =================
        [HttpGet]
        public async Task<ActionResult<object>> GetDailySummary(
            DateTime? startDate,
            DateTime? endDate,
            int? cycleId,
            int skipCount = 0,
            int maxResultCount = 7) // افتراضي 7
        {
            // تحديد بداية ونهاية اليوم
            var fromDate = startDate?.Date ?? DateTime.Today.AddDays(-7);
            var toDate = (endDate?.Date ?? DateTime.Today).AddDays(1).AddTicks(-1);

            // جلب الدورات ضمن الفترة المطلوبة
            var cyclesQuery = _context.Cycles
                .Where(c => c.StartDate <= toDate && c.EndDate >= fromDate);

            if (cycleId.HasValue)
                cyclesQuery = cyclesQuery.Where(c => c.Id == cycleId.Value);

            var cycles = await cyclesQuery
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            // جلب السجلات ضمن الفترة
            var dailyRecords = await _context.DailyRecords
                .Where(d => d.Date >= fromDate && d.Date <= toDate)
                .Include(d => d.FeedConsumptions)
                .ToListAsync();

            var eggProductions = await _context.EggProductionRecords
                .Where(e => e.Date >= fromDate && e.Date <= toDate)
                .ToListAsync();

            var eggSales = await _context.EggSales
                .Where(s => s.Date >= fromDate && s.Date <= toDate)
                .ToListAsync();

            // تجميع الملخص
            var summaryList = new List<DailySummaryDto>();

            for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
            {
                foreach (var cycle in cycles)
                {
                    var dailyRecord = dailyRecords
                        .FirstOrDefault(d => d.CycleId == cycle.Id && d.Date.Date == date);

                    var eggProduction = eggProductions
                        .FirstOrDefault(e => e.CycleId == cycle.Id && e.Date.Date == date);

                    var eggsSold = eggSales
                        .Where(s => s.Date.Date == date)
                        .Sum(s => s.Quantity);

                    summaryList.Add(new DailySummaryDto
                    {
                        Date = date,
                        DayName = date.DayOfWeek.ToString(),
                        CycleName = cycle.Name,
                        ChickAge = dailyRecord?.ChickAge ?? 0,
                        DeadCount = dailyRecord?.DeadCount ?? 0,
                        FeedConsumed = (int)(dailyRecord?.FeedConsumptions.Sum(f => f.Quantity) ?? 0),
                        EggsGood = eggProduction?.TotalEggs ?? 0,
                        EggsBroken = 0,
                        EggsDouble = 0,
                        EggsTotal = eggProduction?.TotalEggs ?? 0,
                        EggsSold = eggsSold
                    });
                }
            }

            // ترتيب السجلات
            var orderedList = summaryList
                .OrderByDescending(s => s.Date)
                .ThenBy(s => s.CycleName)
                .ToList();

            // تطبيق البجنيشن
            var pagedItems = orderedList
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToList();

            return Ok(new
            {
                items = pagedItems,
                totalCount = orderedList.Count,
                fullResult = false
            });
        }

        // ================= DELETE All Data =================
        [HttpDelete("delete-all")]
        public async Task<ActionResult> DeleteAllDailyData()
        {
            _context.DailyRecords.RemoveRange(_context.DailyRecords);
            _context.EggProductionRecords.RemoveRange(_context.EggProductionRecords);
            _context.EggSales.RemoveRange(_context.EggSales);

            await _context.SaveChangesAsync();

            return Ok(new { message = "تم مسح كل البيانات بنجاح" });
        }
    }
}