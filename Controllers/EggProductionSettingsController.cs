using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.DTOs;
using FarmManagement.API.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        // ===== Create Batch with Auto TargetPerBird =====
        [HttpPost]
        public async Task<IActionResult> CreateBatch(EggProductionBatchDto dto)
        {
            if (dto.Weeks == null || !dto.Weeks.Any())
                return BadRequest("No weeks provided.");

            var settings = new List<EggProductionSetting>();

            foreach (var w in dto.Weeks)
            {
                // ===== متوسط عدد الطيور الحية للأسبوع =====
                var avgLiveBirds = await _context.DailyRecords
                    .Where(d => d.Cycle.BreedId == dto.BreedId &&
                                d.ChickAge >= w.WeekStart &&
                                d.ChickAge <= w.WeekEnd)
                    .AverageAsync(d => (decimal?)d.RemainingChicks) ?? 0;

                // ===== حساب TargetPerBird =====
                var targetEggs = (w.TargetProductionPercent / 100m) * avgLiveBirds;
                var targetPerBird = avgLiveBirds == 0 ? 0 : targetEggs / avgLiveBirds;

                settings.Add(new EggProductionSetting
                {
                    BreedId = dto.BreedId,
                    WeekStart = w.WeekStart,
                    WeekEnd = w.WeekEnd,
                    TargetProductionPercent = w.TargetProductionPercent,
                    TargetPerBird = Math.Round(targetPerBird, 2)
                });
            }

            _context.EggProductionSettings.AddRange(settings);
            await _context.SaveChangesAsync();

            return Ok(settings);
        }

        // ===== Get By Breed =====
        [HttpGet("by-breed/{breedId}")]
        public async Task<IActionResult> GetByBreed(int breedId)
        {
            var data = await _context.EggProductionSettings
                .Where(x => x.BreedId == breedId)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return Ok(data);
        }
    }
}