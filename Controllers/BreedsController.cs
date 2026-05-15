using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BreedsController : ControllerBase
    {
        private readonly FarmDbContext _context;
        public BreedsController(FarmDbContext context) => _context = context;

        // ================= GET مع Pagination =================
        [HttpGet]
        public async Task<ActionResult> GetBreeds(int SkipCount = 0, int MaxResultCount = 7)
        {
            var query = _context.Breeds.OrderByDescending(b => b.Id);
            var totalCount = await query.CountAsync();
            var list = await query.Skip(SkipCount).Take(MaxResultCount).ToListAsync();
            return Ok(new
            {
                TotalCount = totalCount,
                Breeds = list
            });
        }


        // ================= CREATE =================
        [HttpPost]
        public async Task<ActionResult> AddBreed([FromBody] BreedDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Breed name is required");

            var breed = new Breed { Name = dto.Name.Trim() };
            _context.Breeds.Add(breed);
            await _context.SaveChangesAsync();
            return Ok(breed);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBreed(int id)
        {
            var breed = await _context.Breeds.FindAsync(id);

            if (breed == null)
                return NotFound(new { message = "السلالة غير موجودة." });

            //  التحقق من الدورات
            var usedInCycles = await _context.Cycles
                .AnyAsync(c => c.BreedId == id);

            //  التحقق من FeedConsumptionSettings
            var usedInFeedSettings = await _context.FeedConsumptionSettings
                .AnyAsync(x => x.BreedId == id);

            //  التحقق من EggProductionSettings
            var usedInEggSettings = await _context.EggProductionSettings
                .AnyAsync(x => x.BreedId == id);

            //  التحقق من TargetMortalitySettings
            var usedInMortalitySettings = await _context.TargetMortalitySettings
                .AnyAsync(x => x.BreedId == id);

            if (usedInCycles || usedInFeedSettings || usedInEggSettings || usedInMortalitySettings)
            {
                return BadRequest(new
                {
                    message = "لا يمكن حذف السلالة لأنها مستخدمة في النظام.",
                    details = new
                    {
                        cycles = usedInCycles,
                        feedConsumption = usedInFeedSettings,
                        eggProduction = usedInEggSettings,
                        mortality = usedInMortalitySettings
                    }
                });
            }

            _context.Breeds.Remove(breed);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف السلالة بنجاح.",
                id
            });
        }
    }
}