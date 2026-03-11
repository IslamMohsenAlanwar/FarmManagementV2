using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public ItemsController(FarmDbContext context)
        {
            _context = context;
        }

        // ================= GET ALL =================[HttpGet]
        public async Task<ActionResult> GetItems(
            int? itemType = null,
            int SkipCount = 0,
            int MaxResultCount = 7)
        {
            var query = _context.Items.AsQueryable();

            if (itemType.HasValue)
            {
                var type = (ItemType)itemType.Value;
                query = query.Where(i => i.ItemType == type);
            }

            query = query.OrderByDescending(i => i.Id);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    PricePerTon = i.PricePerTon,
                    ItemType = i.ItemType
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Items = items
            });
        }

        // ================= GET BY ID =================
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetItem(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();

            return new ItemDto
            {
                Id = item.Id,
                Name = item.Name,
                PricePerTon = item.PricePerTon,
                ItemType = item.ItemType
            };
        }

        // ================= POST =================
        [HttpPost]
        public async Task<ActionResult<ItemDto>> CreateItem(ItemCreateDto dto)
        {
            var item = new Item
            {
                Name = dto.Name,
                PricePerTon = dto.PricePerTon,
                ItemType = dto.ItemType
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();


            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, new ItemDto
            {
                Id = item.Id,
                Name = item.Name,
                PricePerTon = item.PricePerTon,
                ItemType = item.ItemType
            });
        }

        // ================= PUT =================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, ItemUpdateDto dto)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();

            item.Name = dto.Name;
            item.PricePerTon = dto.PricePerTon;
            item.ItemType = dto.ItemType;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ================= DELETE =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
