using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EggSalesController : ControllerBase
    {
        private readonly FarmDbContext _context;
        public EggSalesController(FarmDbContext context) => _context = context;

        // ================= POST: Create Sale =================
        [HttpPost]
        public async Task<ActionResult> CreateSale(EggSaleCreateDto dto)
        {
            
            var trader = await _context.Traders
                .OrderBy(t => t.Id)
                .FirstOrDefaultAsync(t => t.Id == dto.TraderId);

            if (trader == null || trader.Type != TraderType.مشتري)
                return BadRequest("يجب اختيار تاجر مسجل كـ (مشتري بيض).");

       
            var eggItem = await _context.Items
                .Where(i => i.ItemType == ItemType.Egg)
                .OrderBy(i => i.Id)
                .FirstOrDefaultAsync();

            if (eggItem == null)
                return BadRequest("صنف (البيض) غير معرف في قائمة الأصناف العامة.");

            
            var warehouseItem = await _context.WarehouseItems
                .Where(wi => wi.WarehouseId == dto.WarehouseId && wi.ItemId == eggItem.Id)
                .OrderBy(wi => wi.Id)
                .FirstOrDefaultAsync();

            decimal quantityDecimal = dto.Quantity;

            if (warehouseItem == null || warehouseItem.Quantity < quantityDecimal)
                return BadRequest($"الرصيد غير كافٍ. المتاح: {(warehouseItem?.Quantity ?? 0)} كرتونة.");

            decimal totalAmount = quantityDecimal * dto.UnitPrice;
            decimal remaining = totalAmount - dto.PaidAmount;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var sale = new EggSale
                {
                    WarehouseId = dto.WarehouseId,
                    TraderId = dto.TraderId,
                    Date = dto.Date,
                    Quantity = dto.Quantity,
                    UnitPrice = dto.UnitPrice,
                    TotalPrice = totalAmount,
                    PaidAmount = dto.PaidAmount,
                    RemainingAmount = remaining,
                    Notes = dto.Notes
                };
                _context.EggSales.Add(sale);

                warehouseItem.Quantity -= quantityDecimal;
                warehouseItem.Withdrawn += quantityDecimal;

                trader.Balance += remaining;

                _context.WarehouseTransactions.Add(new WarehouseTransaction
                {
                    WarehouseId = dto.WarehouseId,
                    ItemId = eggItem.Id,
                    TraderId = dto.TraderId,
                    Quantity = quantityDecimal,
                    PricePerTon = dto.UnitPrice,
                    TotalPrice = totalAmount,
                    TransactionType = "Sale",
                    Date = dto.Date,
                    EggSale = sale
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "تمت عملية البيع بنجاح",
                    totalPrice = totalAmount,
                    currentTraderBalance = trader.Balance
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest("خطأ في تنفيذ العملية: " + ex.Message);
            }
        }

        // ================= GET: Egg Sale Response =================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EggSaleResponseDto>>> GetSales()
        {
            var sales = await _context.EggSales
                .Include(s => s.Trader)
                .Include(s => s.Warehouse)
                .OrderByDescending(s => s.Date)
                .Select(s => new EggSaleResponseDto
                {
                    Id = s.Id,
                    TraderName = s.Trader != null ? s.Trader.Name : "غير معروف",
                    WarehouseName = s.Warehouse != null ? s.Warehouse.Name : "غير معروف",
                    Date = s.Date,
                    Quantity = s.Quantity,
                    UnitPrice = s.UnitPrice,
                    TotalPrice = s.TotalPrice,
                    PaidAmount = s.PaidAmount,
                    RemainingAmount = s.RemainingAmount,
                    CumulativeBalance = s.Trader != null ? s.Trader.Balance : 0
                })
                .ToListAsync();

            return Ok(sales);
        }
    }
}
