using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.DTOs;
using FarmManagement.API.Models;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EggProductionSettingsController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public EggProductionSettingsController(FarmDbContext context)
        {
            _context = context;
        }

        // ================= CREATE BATCH =================
        [HttpPost]
        public async Task<IActionResult> CreateBatch(EggProductionBatchDto dto)
        {
            if (dto.Weeks == null || !dto.Weeks.Any())
                return BadRequest("No weeks provided.");

            var settings = new List<EggProductionSetting>();

            // ================= Load data once =================
            var records = await _context.DailyRecords
                .Where(d => d.Cycle.BreedId == dto.BreedId)
                .Select(d => new
                {
                    d.ChickAge,
                    d.RemainingChicks
                })
                .ToListAsync();

            foreach (var w in dto.Weeks)
            {
                // ================= Filter by week =================
                var filtered = records
                    .Where(d =>
                    {
                        var weekNumber = (int)Math.Ceiling(d.ChickAge / 7.0);
                        return weekNumber >= w.WeekStart && weekNumber <= w.WeekEnd;
                    })
                    .ToList();

                // ================= Safe average =================
                var avgLiveBirds = filtered.Count > 0
                    ? filtered.Average(x => (decimal)x.RemainingChicks)
                    : 0m;

                // fallback لو مفيش بيانات
                if (avgLiveBirds == 0)
                {
                    avgLiveBirds = records.Any()
                        ? records.Average(x => (decimal)x.RemainingChicks)
                        : 1m;
                }

                var targetProductionPercent = (decimal)w.TargetProductionPercent;

                // ================= (Optional calculation only for validation/debug) =================
                var targetEggs = (targetProductionPercent / 100m) * avgLiveBirds;

                settings.Add(new EggProductionSetting
                {
                    BreedId = dto.BreedId,
                    WeekStart = w.WeekStart,
                    WeekEnd = w.WeekEnd,
                    TargetProductionPercent = targetProductionPercent
                });
            }

            _context.EggProductionSettings.AddRange(settings);
            await _context.SaveChangesAsync();

            return Ok(settings);
        }

        // ================= GET BY BREED =================
        [HttpGet("by-breed/{breedId}")]
        public async Task<IActionResult> GetByBreed(
     int breedId,
     [FromQuery] int SkipCount = 0,
     [FromQuery] int MaxResultCount = 7)
        {
            var query = _context.EggProductionSettings
                .Where(x => x.BreedId == breedId);

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.WeekStart)
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .ToListAsync();

            return Ok(new
            {
                totalCount,
                SkipCount,
                MaxResultCount,
                data
            });
        }
    }
}