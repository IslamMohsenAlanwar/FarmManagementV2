using Microsoft.AspNetCore.Mvc;
using FarmManagement.API.Helpers;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;
using BCrypt.Net;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedConsumptionSettingsController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public FeedConsumptionSettingsController(FarmDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBatch(FeedConsumptionBatchDto dto)
        {
            if (dto.Weeks == null || !dto.Weeks.Any())
                return BadRequest("No weeks provided.");

            var settings = dto.Weeks.Select(week => new FeedConsumptionSetting
            {
                BreedId = dto.BreedId,
                WeekStart = week.WeekStart,
                WeekEnd = week.WeekEnd,
                TargetPerBirdGram = week.TargetPerBirdGram
            }).ToList();

            _context.FeedConsumptionSettings.AddRange(settings);
            await _context.SaveChangesAsync();

            return Ok(settings);
        }


        [HttpGet("by-breed/{breedId}")]
        public async Task<IActionResult> GetByBreed(
     int breedId,
     [FromQuery] int SkipCount = 0,
     [FromQuery] int MaxResultCount = 7)
        {
            var query = _context.FeedConsumptionSettings
                .Where(x => x.BreedId == breedId);

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.Id)
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


        [HttpGet("feed-report")]
        public async Task<ActionResult> GetFeedReport(
    [FromQuery] int cycleId,
    [FromQuery] int SkipCount = 0,
    [FromQuery] int MaxResultCount = 7)
        {
            var cycle = await _context.Cycles
                .Include(c => c.DailyRecords)
                .ThenInclude(d => d.FeedConsumptions)
                .FirstOrDefaultAsync(c => c.Id == cycleId);

            if (cycle == null)
                return BadRequest("Cycle not found.");

            var report = new List<FeedReportDto>();

            // =========================
            // CUMULATIVE (GRAM BASIS)
            // =========================
            decimal cumulativeTargetPerBirdGram = 0;
            decimal cumulativeActualPerBirdGram = 0;

            decimal cumulativeTargetHouseTon = 0;
            decimal cumulativeActualHouseTon = 0;

            // settings once
            var settings = await _context.FeedConsumptionSettings
                .Where(t => t.BreedId == cycle.BreedId)
                .ToListAsync();

            foreach (var record in cycle.DailyRecords.OrderBy(d => d.ChickAge))
            {
                // week calc
                var weekNumber = (record.ChickAge / 7) + 1;

                var targetPerBirdGram = settings
                    .FirstOrDefault(s => s.WeekStart <= weekNumber && s.WeekEnd >= weekNumber)
                    ?.TargetPerBirdGram ?? 0;

                // =========================
                // FEED TOTAL (TON)
                // =========================
                var actualFeedTon = record.FeedConsumptions.Sum(f => f.Quantity);

                // =========================
                // PER BIRD (GRAM)
                // =========================
                var actualPerBirdGram = record.RemainingChicks == 0
                    ? 0
                    : (actualFeedTon * 1_000_000m) / record.RemainingChicks;

                // cumulative (GRAM)
                cumulativeTargetPerBirdGram += targetPerBirdGram;
                cumulativeActualPerBirdGram += actualPerBirdGram;

                var achievementPerBird = targetPerBirdGram == 0
                    ? 0
                    : (actualPerBirdGram / targetPerBirdGram) * 100;

                var cumulativeAchievementPerBird = cumulativeTargetPerBirdGram == 0
                    ? 0
                    : (cumulativeActualPerBirdGram / cumulativeTargetPerBirdGram) * 100;

                // =========================
                // HOUSE (TON)
                // =========================
                var targetHouseTon = (targetPerBirdGram * record.RemainingChicks) / 1_000_000m;

                // FIXED: actualFeedTon already TON
                var actualHouseTon = actualFeedTon;

                cumulativeTargetHouseTon += targetHouseTon;
                cumulativeActualHouseTon += actualHouseTon;

                var achievementHouse = targetHouseTon == 0
                    ? 0
                    : (actualHouseTon / targetHouseTon) * 100;

                var cumulativeAchievementHouse = cumulativeTargetHouseTon == 0
                    ? 0
                    : (cumulativeActualHouseTon / cumulativeTargetHouseTon) * 100;

                report.Add(new FeedReportDto
                {
                    ChickAge = record.ChickAge,

                    TargetFeedPerBirdGram = targetPerBirdGram,
                    ActualFeedPerBirdGram = Math.Round(actualPerBirdGram, 0),

                    AchievementPerBirdPercent = Math.Round(achievementPerBird, 2),
                    CumulativeTargetFeedPerBirdKg = Math.Round(cumulativeTargetPerBirdGram / 1000m, 3),
                    CumulativeActualFeedPerBirdKg = Math.Round(cumulativeActualPerBirdGram / 1000m, 3),
                    CumulativeAchievementPerBirdPercent = Math.Round(cumulativeAchievementPerBird, 2),

                    TargetFeedPerHouseTon = Math.Round(targetHouseTon, 3),
                    ActualFeedPerHouseTon = Math.Round(actualHouseTon, 3),

                    AchievementHousePercent = Math.Round(achievementHouse, 2),
                    CumulativeTargetFeedHouseTon = Math.Round(cumulativeTargetHouseTon, 3),
                    CumulativeActualFeedHouseTon = Math.Round(cumulativeActualHouseTon, 3),

                    CumulativeAchievementHousePercent = Math.Round(cumulativeAchievementHouse, 2)
                });
            }

            var paginatedItems = report
                .OrderByDescending(r => r.ChickAge)
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .ToList();

            return Ok(new
            {
                TotalCount = report.Count,
                Items = paginatedItems
            });
        }


    }
}



