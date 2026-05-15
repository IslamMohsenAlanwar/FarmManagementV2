using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.DTOs;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using System.Globalization;

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

        [HttpGet]
        public async Task<ActionResult<object>> GetDailySummary(
            DateTime? startDate,
            DateTime? endDate,
            int? cycleId,
            int skipCount = 0,
            int maxResultCount = 7)
        {
            var fromDate = startDate?.Date ?? DateTime.Today.AddDays(-7);
            var toDate = (endDate?.Date ?? DateTime.Today).AddDays(1).AddTicks(-1);

            // ================= Cycles =================
            var cyclesQuery = _context.Cycles
                .Where(c => c.StartDate <= toDate && c.EndDate >= fromDate);

            if (cycleId.HasValue)
                cyclesQuery = cyclesQuery.Where(c => c.Id == cycleId.Value);

            var cycles = await cyclesQuery
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            // ================= Data Load =================
            var dailyRecords = await _context.DailyRecords
                .Where(d => d.Date >= fromDate && d.Date <= toDate)
                .Include(d => d.FeedConsumptions)
                .ToListAsync();

            var eggProductions = await _context.EggProductionRecords
                .Include(e => e.Details)
                .Where(e => e.Date >= fromDate && e.Date <= toDate)
                .ToListAsync();

            var eggSales = await _context.EggSales
                .Where(s => s.Date >= fromDate && s.Date <= toDate)
                .ToListAsync();

            // ================= Grouping (IMPORTANT FIX) =================
            var dailyGrouped = dailyRecords
                .GroupBy(d => new { d.CycleId, Date = d.Date.Date })
                .ToDictionary(g => (g.Key.CycleId, g.Key.Date), g => g.First());

            var eggGrouped = eggProductions
                .GroupBy(e => new { e.CycleId, Date = e.Date.Date })
                .ToDictionary(g => (g.Key.CycleId, g.Key.Date), g => g.ToList());

            var salesGrouped = eggSales
                .GroupBy(s => s.Date.Date)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            var summaryList = new List<DailySummaryDto>();

            // ================= Processing =================
            for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
            {
                foreach (var cycle in cycles)
                {
                    dailyGrouped.TryGetValue((cycle.Id, date), out var dailyRecord);
                    eggGrouped.TryGetValue((cycle.Id, date), out var eggRecords);
                    salesGrouped.TryGetValue(date, out var eggsSold);

                    decimal eggsGood = 0;
                    decimal eggsBroken = 0;
                    decimal eggsDouble = 0;
                    decimal eggsFarza = 0;

                    if (eggRecords != null && eggRecords.Any())
                    {
                        var details = eggRecords.SelectMany(e => e.Details);

                        eggsGood = details
                            .Where(d => d.EggQuality == EggQualityType.Normal)
                            .Sum(d => d.CartonsCount);

                        eggsBroken = details
                            .Where(d => d.EggQuality == EggQualityType.Broken)
                            .Sum(d => d.CartonsCount);

                        eggsDouble = details
                            .Where(d => d.EggQuality == EggQualityType.Double)
                            .Sum(d => d.CartonsCount);

                        eggsFarza = details
                            .Where(d => d.EggQuality == EggQualityType.Farza)
                            .Sum(d => d.CartonsCount);
                    }

                    summaryList.Add(new DailySummaryDto
                    {
                        Date = date,
                        DayName = date.ToString("dddd", new CultureInfo("ar-EG")),
                        CycleName = cycle.Name,

                        ChickAge = dailyRecord?.ChickAge ?? 0,
                        DeadCount = dailyRecord?.DeadCount ?? 0,

                        FeedConsumed = dailyRecord?.FeedConsumptions.Sum(f => f.Quantity) ?? 0,

                        EggsGood = eggsGood,
                        EggsBroken = eggsBroken,
                        EggsDouble = eggsDouble,
                        EggsFarza = eggsFarza,
                        EggsTotal = eggsGood + eggsBroken + eggsDouble + eggsFarza,

                        EggsSold = eggsSold
                    });
                }
            }

            // ================= Sorting + Paging =================
            var orderedList = summaryList
                .OrderByDescending(s => s.Date)
                .ThenBy(s => s.CycleName)
                .ToList();

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