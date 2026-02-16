using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FarmsController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public FarmsController(FarmDbContext context)
        {
            _context = context;
        }

        // ================= GET ALL =================
      [HttpGet]
public async Task<ActionResult<IEnumerable<FarmDto>>> GetFarms()
{
    var farms = await _context.Farms
        .Include(f => f.Barns)
        .OrderByDescending(f => f.Id)  
        .Select(f => new FarmDto
        {
            Id = f.Id,
            Name = f.Name,
            Description = f.Description,
            BarnNames = f.Barns.Select(b => b.Name).ToList()
        })
        .ToListAsync();

    return farms;
}


        // ================= GET BY ID =================
        [HttpGet("{id}")]
        public async Task<ActionResult<FarmDto>> GetFarm(int id)
        {
            var farm = await _context.Farms
                .Include(f => f.Barns)
                .Where(f => f.Id == id)
                .Select(f => new FarmDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    BarnNames = f.Barns.Select(b => b.Name).ToList()
                })
                .FirstOrDefaultAsync();

            if (farm == null) return NotFound();

            return farm;
        }

        // ================= POST =================
        [HttpPost]
        public async Task<ActionResult<FarmDto>> CreateFarm(FarmCreateDto dto)
        {
            var farm = new Farm
            {
                Name = dto.Name,
                Description = dto.Description ?? string.Empty
            };

            _context.Farms.Add(farm);
            await _context.SaveChangesAsync();

            var result = new FarmDto
            {
                Id = farm.Id,
                Name = farm.Name,
                Description = farm.Description,
                BarnNames = new List<string>()
            };

            return CreatedAtAction(nameof(GetFarm), new { id = farm.Id }, result);
        }

        // ================= PUT =================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFarm(int id, FarmUpdateDto dto)
        {
            var farm = await _context.Farms.FindAsync(id);
            if (farm == null) return NotFound();

            farm.Name = dto.Name;
            farm.Description = dto.Description ?? farm.Description; 
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ================= DELETE =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFarm(int id)
        {
            var farm = await _context.Farms
                .Include(f => f.Barns)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (farm == null) return NotFound();

            if (farm.Barns.Any())
                return BadRequest("Cannot delete farm that contains barns.");

            _context.Farms.Remove(farm);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
