using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradersController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public TradersController(FarmDbContext context)
        {
            _context = context;
        }

        // ================= GET ALL (With Optional Type Filter) =================
[HttpGet]
public async Task<ActionResult<IEnumerable<TraderDto>>> GetTraders([FromQuery] TraderType? type)
{
    var query = _context.Traders.AsQueryable();

    if (type.HasValue)
        query = query.Where(t => t.Type == type.Value);

    var result = await query
        .OrderByDescending(t => t.Id) 
        .Select(t => new TraderDto
        {
            Id = t.Id,
            Name = t.Name,
            Mobile = t.Mobile,
            Type = t.Type,
            TypeName = t.Type.ToString(),
            Balance = t.Balance
        })
        .ToListAsync();

    return Ok(result);
}


        // ================= GET BY ID =================
        [HttpGet("{id}")]
        public async Task<ActionResult<TraderDto>> GetTrader(int id)
        {
            var trader = await _context.Traders.FindAsync(id);
            if (trader == null) return NotFound();

            return new TraderDto
            {
                Id = trader.Id,
                Name = trader.Name,
                Mobile = trader.Mobile,
                Type = trader.Type,
                TypeName = trader.Type.ToString(),
                Balance = trader.Balance
            };
        }

        // ================= POST =================
        [HttpPost]
        public async Task<ActionResult<TraderDto>> CreateTrader(TraderCreateDto dto)
        {
            var trader = new Trader
            {
                Name = dto.Name,
                Mobile = dto.Mobile,
                Type = dto.Type,
                Balance = dto.Balance
            };

            _context.Traders.Add(trader);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTrader), new { id = trader.Id }, new TraderDto
            {
                Id = trader.Id,
                Name = trader.Name,
                Mobile = trader.Mobile,
                Type = trader.Type,
                TypeName = trader.Type.ToString(),
                Balance = trader.Balance
            });
        }

        // ================= PUT =================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrader(int id, TraderUpdateDto dto)
        {
            var trader = await _context.Traders.FindAsync(id);
            if (trader == null) return NotFound();

            trader.Name = dto.Name;
            trader.Mobile = dto.Mobile;
            trader.Type = dto.Type;
            trader.Balance = dto.Balance;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ================= DELETE =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrader(int id)
        {
            var trader = await _context.Traders.FindAsync(id);
            if (trader == null) return NotFound();

            var hasTransactions = await _context.WarehouseTransactions
                .AnyAsync(t => t.TraderId == id);

            if (hasTransactions)
                return BadRequest("لا يمكن حذف التاجر لوجود معاملات مسجلة باسمه.");

            _context.Traders.Remove(trader);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
