using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChickenSalesController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public ChickenSalesController(FarmDbContext context)
        {
            _context = context;
        }

        // ================= CREATE SALE =================
        [HttpPost]
        public async Task<IActionResult> CreateSale(ChickenSaleCreateDto dto)
        {
            // التاجر
            var trader = await _context.Traders
                .FirstOrDefaultAsync(t => t.Id == dto.TraderId);

            if (trader == null || trader.Type != TraderType.مشتري)
                return BadRequest("يجب اختيار تاجر مسجل كمشتري.");

            // الدورة
            var cycle = await _context.Cycles
                .FirstOrDefaultAsync(c => c.Id == dto.CycleId);

            if (cycle == null)
                return BadRequest("الدورة غير موجودة.");

            // 🔥 آخر سجل يومي
            var lastDailyRecord = await _context.DailyRecords
                .Where(d => d.CycleId == dto.CycleId)
                .OrderByDescending(d => d.Date)
                .FirstOrDefaultAsync();

            if (lastDailyRecord == null)
                return BadRequest("لا يوجد سجل يومي لهذه الدورة.");

            int currentAlive = lastDailyRecord.RemainingChicks;

            // إجمالي المباع قبل كده
            int totalSold = await _context.ChickenSales
                .Where(s => s.CycleId == dto.CycleId)
                .SumAsync(s => s.Quantity);

            int available = currentAlive - totalSold;

            if (dto.Quantity > available)
                return BadRequest($"الرصيد غير كافٍ. المتاح: {available} فرخة.");

            decimal total = dto.Quantity * dto.UnitPrice;
            decimal remaining = total - dto.PaidAmount;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var sale = new ChickenSale
                {
                    CycleId = dto.CycleId,
                    TraderId = dto.TraderId,
                    Date = dto.Date,
                    Quantity = dto.Quantity,
                    UnitPrice = dto.UnitPrice,
                    TotalPrice = total,
                    PaidAmount = dto.PaidAmount,
                    RemainingAmount = remaining,
                    Notes = dto.Notes
                };

                _context.ChickenSales.Add(sale);

                // تحديث رصيد التاجر
                trader.Balance += remaining;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "تم بيع الفراخ بنجاح",
                    availableAfterSale = available - dto.Quantity,
                    traderBalance = trader.Balance
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest("خطأ: " + ex.Message);
            }
        }

        // ================= GET SALES =================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChickenSaleResponseDto>>> GetSales()
        {
            var sales = await _context.ChickenSales
                .Include(s => s.Trader)
                .Include(s => s.Cycle)
                .OrderByDescending(s => s.Date)
                .Select(s => new ChickenSaleResponseDto
                {
                    Id = s.Id,
                    TraderName = s.Trader.Name,
                    CycleName = s.Cycle.Name,
                    Date = s.Date,
                    Quantity = s.Quantity,
                    UnitPrice = s.UnitPrice,
                    TotalPrice = s.TotalPrice,
                    PaidAmount = s.PaidAmount,
                    RemainingAmount = s.RemainingAmount,
                    TraderBalance = s.Trader.Balance
                })
                .ToListAsync();

            return Ok(sales);
        }
    }
}
