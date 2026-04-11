using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.DTOs;
using FarmManagement.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EggProductionReportController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public EggProductionReportController(FarmDbContext context)
        {
            _context = context;
        }

        [HttpGet("egg-report")]
        public async Task<ActionResult> GetEggReport(
            [FromQuery] int cycleId,
            [FromQuery] int SkipCount = 0,
            [FromQuery] int MaxResultCount = 7)
        {
            var cycle = await _context.Cycles
                .Include(c => c.DailyRecords)
                .Include(c => c.EggProductionRecords)
                    .ThenInclude(e => e.Details)
                .FirstOrDefaultAsync(c => c.Id == cycleId);

            if (cycle == null)
                return BadRequest("Cycle not found");

            var report = new List<EggReportDto>();

            decimal cumulativeActualCartons = 0;
            decimal cumulativeTargetCartons = 0;

            // ================== 🔥 1 QUERY ONLY ==================
            var settings = await _context.EggProductionSettings
                .Where(x => x.BreedId == cycle.BreedId)
                .ToListAsync();

            foreach (var day in cycle.DailyRecords.OrderBy(d => d.ChickAge))
            {
                var weekNumber = (int)Math.Ceiling(day.ChickAge / 7.0);

                var eggRecord = cycle.EggProductionRecords
                    .FirstOrDefault(e => e.Date.Date == day.Date.Date);

                decimal broken = 0;
                decimal doubleEgg = 0;
                decimal normal = 0;

                if (eggRecord != null)
                {
                    broken = eggRecord.Details
                        .Where(d => d.EggQuality == EggQualityType.Broken)
                        .Sum(d => d.CartonsCount);

                    doubleEgg = eggRecord.Details
                        .Where(d => d.EggQuality == EggQualityType.Double)
                        .Sum(d => d.CartonsCount);

                    normal = eggRecord.Details
                        .Where(d => d.EggQuality == EggQualityType.Normal)
                        .Sum(d => d.CartonsCount);
                }

                var totalActual = broken + doubleEgg + normal;
                var liveBirds = day.RemainingChicks;

                // ================= TARGET SETTING (IN MEMORY) =================
                var setting = settings
                    .FirstOrDefault(t =>
                        t.WeekStart <= weekNumber &&
                        t.WeekEnd >= weekNumber);

                var targetPercent = setting?.TargetProductionPercent ?? 0m;

                // ================= DERIVED VALUES =================
                var targetPerBird = targetPercent / 100m;

                var targetEggs = liveBirds * targetPerBird;
                var targetCartons = targetEggs / 30m;

                // ================= ACTUAL =================
                var actualEggs = totalActual * 30m;

                // ================= PERCENTAGES =================
                var actualPercent = liveBirds == 0
                    ? 0
                    : (actualEggs / liveBirds) * 100;

                var achievementPercent = targetPercent == 0
                    ? 0
                    : (actualPercent / targetPercent) * 100;

                // ================= PER BIRD =================
                var actualPerBird = liveBirds == 0
                    ? 0
                    : actualEggs / liveBirds;

                var achievementPerBird = targetPerBird == 0
                    ? 0
                    : (actualPerBird / targetPerBird) * 100;

                // ================= CUMULATIVE =================
                cumulativeActualCartons += totalActual;
                cumulativeTargetCartons += targetCartons;

                var cumulativeAchievement = cumulativeTargetCartons == 0
                    ? 0
                    : (cumulativeActualCartons / cumulativeTargetCartons) * 100;

                // ================= DTO =================
                report.Add(new EggReportDto
                {
                    ChickAge = day.ChickAge,

                    BrokenCartons = broken,
                    DoubleCartons = doubleEgg,
                    NormalCartons = normal,
                    TotalActualCartons = totalActual,

                    TargetCartons = Math.Round(targetCartons, 2),

                    ActualPercent = Math.Round(actualPercent, 2),
                    TargetPercent = Math.Round(targetPercent, 2),
                    AchievementPercent = Math.Round(achievementPercent, 2),

                    TargetPerBird = Math.Round(targetPerBird, 4),

                    ActualPerBird = Math.Round(actualPerBird, 4),
                    AchievementPerBird = Math.Round(achievementPerBird, 2),

                    CumulativeActual = Math.Round(cumulativeActualCartons, 2),
                    CumulativeTarget = Math.Round(cumulativeTargetCartons, 2),
                    CumulativeAchievement = Math.Round(cumulativeAchievement, 2)
                });
            }

            var items = report
                .OrderByDescending(x => x.ChickAge)
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .ToList();

            return Ok(new
            {
                TotalCount = report.Count,
                Items = items
            });
        }
    }
}