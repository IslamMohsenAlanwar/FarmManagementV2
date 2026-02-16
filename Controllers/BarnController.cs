using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarnsController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public BarnsController(FarmDbContext context)
        {
            _context = context;
        }

        // ================= GET ALL =================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BarnDto>>> GetBarns()
        {
            var barns = await _context.Barns
                .Include(b => b.Farm)
                .OrderByDescending(b => b.Id)
                .Select(b => new BarnDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    FarmId = b.FarmId,
                    FarmName = b.Farm.Name,
                    Type = (int)b.Type,
                    TypeName = b.Type == BarnType.Layers ? "بياض" : "تسمين"
                })
                .ToListAsync();

            return barns;
        }

        // ================= GET BY ID =================
        [HttpGet("{id}")]
        public async Task<ActionResult<BarnDto>> GetBarn(int id)
        {
            var barn = await _context.Barns
                .Include(b => b.Farm)
                .Where(b => b.Id == id)
                .Select(b => new BarnDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    FarmId = b.FarmId,
                    FarmName = b.Farm.Name,
                    Type = (int)b.Type,
                    TypeName = b.Type == BarnType.Layers ? "بياض" : "تسمين"
                })
                .FirstOrDefaultAsync();

            if (barn == null) return NotFound();

            return barn;
        }

        // ================= POST =================
        [HttpPost]
        public async Task<ActionResult<BarnDto>> CreateBarn(BarnCreateDto dto)
        {
            var farmExists = await _context.Farms.AnyAsync(f => f.Id == dto.FarmId);
            if (!farmExists) return BadRequest("Farm not found");

            if (!Enum.IsDefined(typeof(BarnType), dto.Type))
                return BadRequest("Invalid barn type");

            var barn = new Barn
            {
                Name = dto.Name,
                FarmId = dto.FarmId,
                Type = (BarnType)dto.Type
            };

            _context.Barns.Add(barn);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBarn), new { id = barn.Id }, new BarnDto
            {
                Id = barn.Id,
                Name = barn.Name,
                FarmId = barn.FarmId,
                FarmName = (await _context.Farms.FindAsync(barn.FarmId))!.Name,
                Type = (int)barn.Type,
                TypeName = barn.Type == BarnType.Layers ? "بياض" : "تسمين"
            });
        }

        // ================= PUT =================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBarn(int id, BarnUpdateDto dto)
        {
            var barn = await _context.Barns.FindAsync(id);
            if (barn == null) return NotFound();

            var farmExists = await _context.Farms.AnyAsync(f => f.Id == dto.FarmId);
            if (!farmExists) return BadRequest("Farm not found");

            if (!Enum.IsDefined(typeof(BarnType), dto.Type))
                return BadRequest("Invalid barn type");

            barn.Name = dto.Name;
            barn.FarmId = dto.FarmId;
            barn.Type = (BarnType)dto.Type;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ================= DELETE =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBarn(int id)
        {
            var barn = await _context.Barns.FindAsync(id);
            if (barn == null) return NotFound();

            _context.Barns.Remove(barn);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
