using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.DTOs;
using FarmManagement.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using FarmManagement.API.Services;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EggProductionReportController : ControllerBase
    {
        private readonly FarmDbContext _context;
        private readonly IEggProductionReportExportService _exportService;

        public EggProductionReportController(
            FarmDbContext context,
            IEggProductionReportExportService exportService)
        {
            _context = context;
            _exportService = exportService;
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

            //  تحميل الإعدادات مرة واحدة
            var settings = await _context.EggProductionSettings
                .Where(x => x.BreedId == cycle.BreedId)
                .ToListAsync();

            //  GROUPING الإنتاج حسب التاريخ (أفضل أداء)
            var eggGrouped = cycle.EggProductionRecords
                .GroupBy(e => e.Date.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var day in cycle.DailyRecords.OrderBy(d => d.ChickAge))
            {
                var weekNumber = (int)Math.Ceiling(day.ChickAge / 7.0);

                //  هات كل الإنتاجات في اليوم
                eggGrouped.TryGetValue(day.Date.Date, out var eggRecords);

                decimal broken = 0;
                decimal doubleEgg = 0;
                decimal normal = 0;
                decimal farza = 0;

                if (eggRecords != null && eggRecords.Any())
                {
                    var allDetails = eggRecords.SelectMany(e => e.Details);

                    broken = allDetails
                        .Where(d => d.EggQuality == EggQualityType.Broken)
                        .Sum(d => d.CartonsCount);

                    doubleEgg = allDetails
                        .Where(d => d.EggQuality == EggQualityType.Double)
                        .Sum(d => d.CartonsCount);

                    normal = allDetails
                        .Where(d => d.EggQuality == EggQualityType.Normal)
                        .Sum(d => d.CartonsCount);

                    farza = allDetails
                        .Where(d => d.EggQuality == EggQualityType.Farza)
                        .Sum(d => d.CartonsCount);
                }

                var totalActual = broken + doubleEgg + normal + farza;
                var liveBirds = day.RemainingChicks;

                // ================= TARGET =================
                var setting = settings.FirstOrDefault(t =>
                    t.WeekStart <= weekNumber &&
                    t.WeekEnd >= weekNumber);

                var targetPercent = setting?.TargetProductionPercent ?? 0m;

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
                    FarzaCartons = farza,
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


        [HttpGet("export-excel/{cycleId}")]
        public async Task<IActionResult> ExportExcel(int cycleId)
        {
            var file = await _exportService.ExportReportExcelAsync(cycleId);

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"egg-report-{cycleId}.xlsx"
            );
        }

        // [HttpGet("export-pdf/{cycleId}")]
        // public async Task<IActionResult> ExportPdf(int cycleId)
        // {
        //     var file = await _exportService.ExportReportPdfAsync(cycleId);

        //     return File(
        //         file,
        //         "application/pdf",
        //         $"egg-report-{cycleId}.pdf"
        //     );
        // }
    }
}