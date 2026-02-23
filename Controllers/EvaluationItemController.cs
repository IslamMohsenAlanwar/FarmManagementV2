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

        // ================= GET all items =================
        [HttpGet]
        public async Task<ActionResult<List<EvaluationItem>>> GetAll()
        {
            return await _context.EvaluationItems.ToListAsync();
        }

        // ================= GET single item =================
        [HttpGet("{id}")]
        public async Task<ActionResult<EvaluationItem>> Get(int id)
        {
            var item = await _context.EvaluationItems.FindAsync(id);
            if (item == null)
                return NotFound();
            return item;
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

            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
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

            _context.EvaluationItems.Update(item);
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