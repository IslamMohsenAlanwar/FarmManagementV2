using Microsoft.AspNetCore.Mvc;
using FarmManagement.API.Helpers;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TargetMortalityController : ControllerBase
    {
        private readonly FarmDbContext _context;
        public TargetMortalityController(FarmDbContext context) => _context = context;

        // ================= GET  =================

        [HttpGet("GetWeeksByBreed")]
        public async Task<ActionResult> GetWeeksByBreed(
    [FromQuery] int breedId,
    int SkipCount = 0,
    int MaxResultCount = 7)
        {
            var query = _context.TargetMortalitySettings
                .Where(t => t.BreedId == breedId)
                .OrderByDescending(t => t.Id);

            var totalCount = await query.CountAsync();

            var list = await query
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .Select(t => new TargetMortalityWeekDto
                {
                    WeekStart = t.WeekStart,
                    WeekEnd = t.WeekEnd,
                    ExpectedMortalityRate = t.ExpectedMortalityRate
                })
                .ToListAsync();

            return Ok(new
            {
                totalCount,
                items = list
            });
        }
        // ================= POST لإضافة مستهدف نافق =================
        [HttpPost("AddMultiple")]
        public async Task<ActionResult> AddMultiple([FromBody] TargetMortalityInputDto dto)
        {
            var breedExists = await _context.Breeds.AnyAsync(b => b.Id == dto.BreedId);
            if (!breedExists)
                return BadRequest("Breed not found");

            var records = dto.Weeks.Select(w => new TargetMortalitySetting
            {
                BreedId = dto.BreedId,
                WeekStart = w.WeekStart,
                WeekEnd = w.WeekEnd,
                ExpectedMortalityRate = w.ExpectedMortalityRate
            }).ToList();

            _context.TargetMortalitySettings.AddRange(records);
            await _context.SaveChangesAsync();

            return Ok(records);
        }
    }

}



