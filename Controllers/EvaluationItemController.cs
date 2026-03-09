using FarmManagement.API.Data;
using FarmManagement.API.DTOs;
using FarmManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluationItemController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public EvaluationItemController(FarmDbContext context)
        {
            _context = context;
        }

        // ================= GET all items with pagination =================
        [HttpGet]
        public async Task<ActionResult> GetAll(
            int skip = 0,
            int take = 7) // القيمة الافتراضية 7
        {
            var query = _context.EvaluationItems.OrderByDescending(e => e.Id);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Items = items
            });
        }

        // ================= GET single item by id =================
        [HttpGet("{id}")]
        public async Task<ActionResult<EvaluationItem>> GetById(int id)
        {
            var item = await _context.EvaluationItems.FindAsync(id);
            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // ================= POST: create new item =================
        [HttpPost]
        public async Task<ActionResult<EvaluationItem>> Create(EvaluationItemDto dto)
        {
            var item = new EvaluationItem
            {
                Name = dto.Name,
                MaxScore = dto.MaxScore
            };

            _context.EvaluationItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        // ================= PUT: update existing item =================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, EvaluationItemDto dto)
        {
            var item = await _context.EvaluationItems.FindAsync(id);
            if (item == null)
                return NotFound();

            item.Name = dto.Name;
            item.MaxScore = dto.MaxScore;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ================= DELETE: remove item =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.EvaluationItems.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.EvaluationItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}