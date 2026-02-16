using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetItemsController : ControllerBase
    {
        private readonly FarmDbContext _context;
        public AssetItemsController(FarmDbContext context) => _context = context;

        // ================= GET =================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssetItemDto>>> GetAll()
        {
            var items = await _context.AssetItems
                .OrderByDescending(a => a.Id)
                .Select(a => new AssetItemDto
                {
                    Id = a.Id,
                    Name = a.Name
                }).ToListAsync();

            return Ok(items);
        }

        // ================= POST =================
        [HttpPost]
        public async Task<ActionResult<AssetItemDto>> Create(CreateAssetItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name cannot be empty.");

            var name = dto.Name.Trim();

            // (Case Insensitive)
            var exists = await _context.AssetItems
                .AnyAsync(a => a.Name.ToLower() == name.ToLower());

            if (exists)
                return BadRequest("Asset name already exists.");

            var item = new AssetItem
            {
                Name = name
            };

            _context.AssetItems.Add(item);
            await _context.SaveChangesAsync();

            var result = new AssetItemDto
            {
                Id = item.Id,
                Name = item.Name
            };

            return CreatedAtAction(nameof(GetAll), new { id = item.Id }, result);
        }

        // ================= PUT =================
        [HttpPut("{id}")]
        public async Task<ActionResult<AssetItemDto>> Update(int id, UpdateAssetItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name cannot be empty.");

            var item = await _context.AssetItems.FindAsync(id);

            if (item == null)
                return NotFound("Asset item not found.");

            var name = dto.Name.Trim();

            var exists = await _context.AssetItems
                .AnyAsync(a => a.Name.ToLower() == name.ToLower() && a.Id != id);

            if (exists)
                return BadRequest("Asset name already exists.");

            item.Name = name;

            await _context.SaveChangesAsync();

            return Ok(new AssetItemDto
            {
                Id = item.Id,
                Name = item.Name
            });
        }
    }
}
