using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;
using FarmManagement.API.Helpers;
using FarmManagement.API.Enums;


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
            // =========================
            // التحقق من العميل
            // =========================
            var trader = await _context.Traders
                .FirstOrDefaultAsync(t => t.Id == dto.TraderId);

            if (trader == null || trader.Type != TraderType.عميل)
                return BadRequest("يجب اختيار تاجر مسجل كـ (عميل).");

            // =========================
            // التحقق من التاريخ
            // =========================
            var saleDate = dto.Date;

            if (saleDate.Date > DateTime.Today)
                return BadRequest("لا يمكن اختيار تاريخ مستقبلي.");

            // =========================
            // جلب صنف البيض
            // =========================
            var eggItem = await _context.Items
                .FirstOrDefaultAsync(i => i.ItemType == ItemType.Egg);

            if (eggItem == null)
                return BadRequest("صنف البيض غير معرف.");

            // =========================
            // جلب المخزون حسب الجودة
            // =========================
            var warehouseItem = await _context.WarehouseItems
                .FirstOrDefaultAsync(wi =>
                    wi.WarehouseId == dto.WarehouseId &&
                    wi.ItemId == eggItem.Id &&
                    wi.EggQuality == dto.EggQuality);

            if (warehouseItem == null)
                return BadRequest($"لا يوجد مخزون للنوع {dto.EggQuality.ToArabic()}.");

            decimal availableQty = warehouseItem.Quantity - warehouseItem.Withdrawn;

            if (availableQty < dto.Quantity)
                return BadRequest(
                    $"الرصيد غير كافٍ للنوع {dto.EggQuality.ToArabic()}. المتاح: {availableQty}");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // =========================
                // إنشاء فاتورة البيع
                // =========================
                var sale = new EggSale
                {
                    WarehouseId = dto.WarehouseId,
                    TraderId = dto.TraderId,
                    Date = saleDate,
                    Quantity = dto.Quantity,
                    UnitPrice = 0,
                    TotalPrice = 0,
                    PaidAmount = dto.PaidAmount,
                    RemainingAmount = 0,
                    Notes = dto.Notes
                };

                _context.EggSales.Add(sale);

                // =========================
                // تحديث المخزون
                // =========================
                warehouseItem.Withdrawn += dto.Quantity;

                // =========================
                // حركة المخزن
                // =========================
                _context.WarehouseTransactions.Add(new WarehouseTransaction
                {
                    WarehouseId = dto.WarehouseId,
                    ItemId = eggItem.Id,
                    TraderId = dto.TraderId,
                    Quantity = dto.Quantity,
                    TransactionType = "Sale",
                    Date = saleDate,
                    EggSale = sale,
                    EggQuality = dto.EggQuality
                });

                // =========================
                // تسجيل الخزنة
                // =========================
                if (dto.PaidAmount > 0)
                {
                    _context.CashBoxTransactions.Add(new CashBoxTransaction
                    {
                        Date = saleDate,
                        Type = CashBoxType.Income,
                        Category = CashBoxCategory.EggSale,
                        Amount = dto.PaidAmount,
                        Notes = $"دفعة من العميل {trader.Name}",
                        TraderId = trader.Id,
                        WarehouseId = dto.WarehouseId
                    });
                }

                // =========================
                // حفظ
                // =========================
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "تم تسجيل البيع بنجاح",
                    soldQuantity = dto.Quantity,
                    paid = dto.PaidAmount,
                    remainingStock = warehouseItem.Quantity - warehouseItem.Withdrawn
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return BadRequest(new
                {
                    message = "حدث خطأ أثناء تسجيل البيع",
                    error = ex.Message
                });
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
        public async Task<ActionResult> GetSales(
            int SkipCount = 0,
            int MaxResultCount = 7)  // القيمة الافتراضية 7
        {
            var query = _context.EggSales
                .Include(s => s.Trader)
                .Include(s => s.Warehouse)
                .Include(s => s.WarehouseTransactions)
                .OrderByDescending(s => s.Id);

            var totalCount = await query.CountAsync();

            var salesList = await query
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .ToListAsync();

            var result = salesList.Select(s => new EggSaleResponseDto
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
            }).ToList();

            return Ok(new
            {
                TotalCount = totalCount,
                Sales = result
            });
        }


        [HttpGet("lite")]
        public async Task<ActionResult> GetSalesLite(
    int SkipCount = 0,
    int MaxResultCount = 7)
        {
            var query = _context.EggSales
                .Include(s => s.Trader)
                .Include(s => s.Warehouse)
                .Include(s => s.WarehouseTransactions)
                .OrderByDescending(s => s.Id);

            var totalCount = await query.CountAsync();

            var salesList = await query
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .ToListAsync();

            var result = salesList.Select(s => new EggSaleLiteDto
            {
                Id = s.Id,
                TraderName = s.Trader != null ? s.Trader.Name : "غير معروف",
                WarehouseName = s.Warehouse != null ? s.Warehouse.Name : "غير معروف",
                Date = s.Date,
                Quantity = s.Quantity,
                EggQuality = s.WarehouseTransactions.FirstOrDefault()?.EggQuality.ToArabic()
            }).ToList();

            return Ok(new
            {
                TotalCount = totalCount,
                Sales = result
            });
        }
    }
}