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
    }
}