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
    public async Task<IActionResult> GetByBreed(int breedId)
    {
        var data = await _context.FeedConsumptionSettings
            .Where(x => x.BreedId == breedId)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return Ok(data);
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

    if (cycle == null) return BadRequest("Cycle not found.");

    var report = new List<FeedReportDto>();

    decimal cumulativeTargetPerBirdKg = 0;
    decimal cumulativeActualPerBirdKg = 0;
    decimal cumulativeTargetHouseTon = 0;
    decimal cumulativeActualHouseTon = 0;

    // الحساب التراكمي تصاعدي
    foreach (var record in cycle.DailyRecords.OrderBy(d => d.DayNumber))
    {
        var targetPerBirdGram = await _context.FeedConsumptionSettings
            .Where(t => t.BreedId == cycle.BreedId &&
                        t.WeekStart <= record.DayNumber &&
                        t.WeekEnd >= record.DayNumber)
            .Select(t => t.TargetPerBirdGram)
            .FirstOrDefaultAsync();

        var actualFeedKg = record.FeedConsumptions.Sum(f => f.Quantity);

        var actualPerBirdGram = record.RemainingChicks == 0 ? 0 : (actualFeedKg * 1000) / record.RemainingChicks;

        cumulativeTargetPerBirdKg += targetPerBirdGram / 1000m;
        cumulativeActualPerBirdKg += actualPerBirdGram / 1000m;

        var achievementPerBird = targetPerBirdGram == 0 ? 0 : (actualPerBirdGram / targetPerBirdGram) * 100;
        var cumulativeAchievementPerBird = cumulativeTargetPerBirdKg == 0 ? 0 :
            (cumulativeActualPerBirdKg / cumulativeTargetPerBirdKg) * 100;

        var targetHouseTon = (targetPerBirdGram * record.RemainingChicks) / 1_000_000m;
        var actualHouseTon = actualFeedKg / 1000m;

        cumulativeTargetHouseTon += targetHouseTon;
        cumulativeActualHouseTon += actualHouseTon;

        var achievementHouse = targetHouseTon == 0 ? 0 : (actualHouseTon / targetHouseTon) * 100;
        var cumulativeAchievementHouse = cumulativeTargetHouseTon == 0 ? 0 :
            (cumulativeActualHouseTon / cumulativeTargetHouseTon) * 100;

        report.Add(new FeedReportDto
        {
            DayNumber = record.DayNumber,
            TargetFeedPerBirdGram = targetPerBirdGram,
            ActualFeedPerBirdGram = actualPerBirdGram,
            AchievementPerBirdPercent = Math.Round(achievementPerBird, 2),
            CumulativeTargetFeedPerBirdKg = Math.Round(cumulativeTargetPerBirdKg, 2),
            CumulativeActualFeedPerBirdKg = Math.Round(cumulativeActualPerBirdKg, 2),
            CumulativeAchievementPerBirdPercent = Math.Round(cumulativeAchievementPerBird, 2),
            TargetFeedPerHouseTon = Math.Round(targetHouseTon, 2),
            ActualFeedPerHouseTon = Math.Round(actualHouseTon, 2),
            AchievementHousePercent = Math.Round(achievementHouse, 2),
            CumulativeTargetFeedHouseTon = Math.Round(cumulativeTargetHouseTon, 2),
            CumulativeActualFeedHouseTon = Math.Round(cumulativeActualHouseTon, 2),
            CumulativeAchievementHousePercent = Math.Round(cumulativeAchievementHouse, 2)
        });
    }

    // ترتيب جديد + Pagination
    var paginatedItems = report
        .OrderByDescending(r => r.DayNumber) 
        .Skip(SkipCount)
        .Take(MaxResultCount)
        .ToList();

    var totalCount = report.Count;

    return Ok(new
    {
        TotalCount = totalCount,
        Items = paginatedItems
    });
}

    
}
}



