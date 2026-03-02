using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.DTOs;
using FarmManagement.API.Data; // DbContext
using FarmManagement.API.Models; // Models

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

        // GET /api/daily-summary?startDate=YYYY-MM-DD&endDate=YYYY-MM-DD
        [HttpGet]
        public async Task<ActionResult<List<DailySummaryDto>>> GetDailySummary(DateTime? startDate, DateTime? endDate)
        {
            var fromDate = startDate ?? DateTime.Today.AddDays(-7); // افتراضي آخر 7 أيام
            var toDate = endDate ?? DateTime.Today;

            // جلب جميع الدورات التي تغطي الفترة المطلوبة
            var cycles = await _context.Cycles
                .Where(c => c.StartDate <= toDate && c.EndDate >= fromDate)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            // جلب جميع سجلات اليوم لهذه الفترة
            var dailyRecords = await _context.DailyRecords
                .Where(d => d.Date.Date >= fromDate.Date && d.Date.Date <= toDate.Date)
                .Include(d => d.FeedConsumptions)
                .ToListAsync();

            // جلب انتاج البيض لهذه الفترة
            var eggProductions = await _context.EggProductionRecords
                .Include(e => e.Details)
                .Where(e => e.Date.Date >= fromDate.Date && e.Date.Date <= toDate.Date)
                .ToListAsync();

            // جلب مبيعات البيض لهذه الفترة
            var eggSales = await _context.EggSales
                .Where(s => s.Date.Date >= fromDate.Date && s.Date.Date <= toDate.Date)
                .ToListAsync();

            var summaryList = new List<DailySummaryDto>();

            // حلقة لكل يوم ولكل دورة
            for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
            {
                foreach (var cycle in cycles)
                {
                    // نتأكد اليوم ضمن فترة الدورة
                    var dailyRecord = dailyRecords.FirstOrDefault(d => d.CycleId == cycle.Id && d.Date.Date == date);
                    var eggProduction = eggProductions.FirstOrDefault(e => e.Id == cycle.Id && e.Date.Date == date);
                    var eggsSold = eggSales.Where(s => s.Id == cycle.Id && s.Date.Date == date).Sum(s => s.Quantity);

                    var summary = new DailySummaryDto
                    {
                        Date = date,
                        DayName = date.DayOfWeek.ToString(),
                        CycleName = cycle.Name,
                        ChickAge = dailyRecord?.ChickAge ?? 0,

                        // هنا نستخدم TotalEggs مباشرة لأنه ما عندنا Type في DTO
                        EggsGood = eggProduction?.TotalEggs ?? 0,
                        EggsBroken = 0,
                        EggsDouble = 0,
                        EggsTotal = eggProduction?.TotalEggs ?? 0,

                        DeadCount = dailyRecord?.DeadCount ?? 0,
                        FeedConsumed = (int)(dailyRecord?.FeedConsumptions.Sum(f => f.Quantity) ?? 0),

                        EggsSold = eggsSold
                    };

                    summaryList.Add(summary);
                }
            }

            // ترتيب النتائج حسب التاريخ ثم اسم الدورة
            return Ok(summaryList.OrderBy(s => s.Date).ThenBy(s => s.CycleName));
        }
    }
}