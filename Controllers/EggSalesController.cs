using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;
using FarmManagement.API.Helpers;

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
            // جلب التاجر
            var trader = await _context.Traders
                .FirstOrDefaultAsync(t => t.Id == dto.TraderId);

            if (trader == null || trader.Type != TraderType.عميل)
                return BadRequest("يجب اختيار تاجر مسجل كـ (مشتري بيض).");

            // جلب صنف البيض
            var eggItem = await _context.Items
                .FirstOrDefaultAsync(i => i.ItemType == ItemType.Egg);

            if (eggItem == null)
                return BadRequest("صنف (البيض) غير معرف.");

            // جلب العنصر في المخزن بناءً على النوع
            var warehouseItem = await _context.WarehouseItems
                .FirstOrDefaultAsync(wi =>
                    wi.WarehouseId == dto.WarehouseId &&
                    wi.ItemId == eggItem.Id &&
                    wi.EggQuality == dto.EggQuality);

            if (warehouseItem == null || warehouseItem.Quantity < dto.Quantity)
                return BadRequest($"الرصيد غير كافٍ للنوع {dto.EggQuality.ToArabic()}. المتاح: {(warehouseItem?.Quantity ?? 0)}");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // إنشاء البيع
                var sale = new EggSale
                {
                    WarehouseId = dto.WarehouseId,
                    TraderId = dto.TraderId,
                    Date = dto.Date,
                    Quantity = dto.Quantity,
                    UnitPrice = 0,
                    TotalPrice = 0,
                    PaidAmount = dto.PaidAmount,
                    RemainingAmount = 0,
                    Notes = dto.Notes
                };

                _context.EggSales.Add(sale);

                // تحديث المخزن
                warehouseItem.Quantity -= dto.Quantity;
                warehouseItem.Withdrawn += dto.Quantity;

                // تسجيل حركة المخزن
                _context.WarehouseTransactions.Add(new WarehouseTransaction
                {
                    WarehouseId = dto.WarehouseId,
                    ItemId = eggItem.Id,
                    TraderId = dto.TraderId,
                    Quantity = dto.Quantity,
                    TransactionType = "Sale",
                    Date = dto.Date,
                    EggSale = sale,
                    EggQuality = dto.EggQuality
                });

                // تسجيل الخزنة (المبلغ المدفوع فقط)
                _context.CashBoxTransactions.Add(new CashBoxTransaction
                {
                    Date = dto.Date,
                    Type = "إيراد",
                    Category = "بيع بيض",
                    Amount = dto.PaidAmount,
                    Notes = $"دفعة مبدئية من {trader.Name}",
                    TraderId = trader.Id,
                    WarehouseId = dto.WarehouseId
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "تم تسجيل البيع المبدئي بنجاح"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        // ================= PUT: Set Price =================
        [HttpPut("{id}/set-price")]
        public async Task<ActionResult> SetPrice(int id, EggSaleUpdatePriceDto dto)
        {
            var sale = await _context.EggSales
                .Include(s => s.Trader)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
                return NotFound("عملية البيع غير موجودة.");

            if (sale.UnitPrice > 0)
                return BadRequest("تم إدخال السعر مسبقًا.");

            var totalPrice = sale.Quantity * dto.UnitPrice;
            var remaining = totalPrice - sale.PaidAmount;

            sale.UnitPrice = dto.UnitPrice;
            sale.TotalPrice = totalPrice;
            sale.RemainingAmount = remaining;

            // تحديث رصيد التاجر
            if (sale.Trader != null)
                sale.Trader.Balance += remaining;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تحديث السعر بنجاح",
                totalPrice,
                remaining,
                traderBalance = sale.Trader?.Balance
            });
        }

// ================= GET: List Sales =================
[HttpGet]
public async Task<ActionResult<IEnumerable<EggSaleResponseDto>>> GetSales()
{
    var sales = await _context.EggSales
        .Include(s => s.Trader)
        .Include(s => s.Warehouse)
        .Include(s => s.WarehouseTransactions)
        .OrderByDescending(s => s.Id)
        .ToListAsync();

    var result = sales.Select(s => new EggSaleResponseDto
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
        CumulativeBalance = s.Trader != null ? s.Trader.Balance : 0,
        EggQuality = s.WarehouseTransactions.FirstOrDefault()?.EggQuality.ToArabic()
    });

    return Ok(result);
}
    }
}