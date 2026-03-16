using Microsoft.AspNetCore.Mvc;
using FarmManagement.API.Helpers;
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

        // ================= GET ALL (With Optional Type Filter) ================
        [HttpGet]
        public async Task<ActionResult> GetTraders(
    [FromQuery] TraderType? type,
    int SkipCount = 0,
    int MaxResultCount = 7)
        {
            var query = _context.Traders.AsQueryable();

            if (type.HasValue)
                query = query.Where(t => t.Type == type.Value);

            var totalCount = await query.CountAsync();

            var traders = await query
                .OrderByDescending(t => t.Id)
                .Skip(SkipCount)
                .Take(MaxResultCount)
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

            return Ok(new
            {
                TotalCount = totalCount,
                Traders = traders
            });
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

        [HttpPost("pay-trader")]
public async Task<ActionResult> PayTrader([FromBody] PayTraderDto dto)
{
    var trader = await _context.Traders.FindAsync(dto.TraderId);
    if (trader == null)
        return BadRequest("المورد غير موجود.");

    var date = dto.Date ?? DateTime.Now;

    await using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        // =========================
        // تحديث الخزنة بالمبلغ المدفوع
        // =========================
        if (dto.Amount > 0 && dto.WarehouseId.HasValue)
        {
            var cashBoxEntry = new CashBoxTransaction
            {
                Date = date,
                Type = "منصرف",
                Category = "دفع لمورد",
                Amount = dto.Amount,
                Notes = dto.Notes ?? $"دفع للمورد {trader.Name}",
                TraderId = trader.Id,
                WarehouseId = dto.WarehouseId.Value
            };
            _context.CashBoxTransactions.Add(cashBoxEntry);
        }

        // =========================
        // تحديث Ledger المورد
        // =========================
        var lastLedger = await _context.TraderLedgers
            .Where(l => l.TraderId == trader.Id)
            .OrderByDescending(l => l.Date)
            .FirstOrDefaultAsync();

        decimal previousBalance = lastLedger?.Balance ?? 0;
        if (dto.Amount > previousBalance)
            return BadRequest($"المبلغ المدفوع أكبر من رصيد المورد الحالي ({previousBalance}).");

        decimal newBalance = previousBalance - dto.Amount;

        var ledgerEntry = new TraderLedger
        {
            TraderId = trader.Id,
            Date = date,
            Debit = 0,
            Credit = dto.Amount,
            Balance = newBalance,
            Notes = dto.Notes ?? $"دفع لمورد"
        };

        _context.TraderLedgers.Add(ledgerEntry);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            message = "تم دفع المبلغ بنجاح",
            paid = dto.Amount,
            remainingBalance = newBalance
        });
    }
    catch (Exception)
    {
        await transaction.RollbackAsync();
        return BadRequest("حدث خطأ أثناء تسجيل الدفع");
    }
}

        [HttpGet("trader/{traderId}/ledger")]
        public async Task<ActionResult> GetTraderLedger(
    int traderId,
    int SkipCount = 0,
    int MaxResultCount = 7)
        {
            var query = _context.TraderLedgers
                .Where(l => l.TraderId == traderId)
                .OrderByDescending(l => l.Id);

            var totalCount = await query.CountAsync();

            var ledger = await query
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Ledger = ledger
            });
        }


    [HttpGet("supplier/{traderId}/invoices")]
     public async Task<ActionResult<SupplierInvoiceReportDto>> GetSupplierInvoices(
    int traderId,
    int SkipCount = 0,
    int MaxResultCount = 10)
        {
            var trader = await _context.Traders.FindAsync(traderId);
            if (trader == null || trader.Type != TraderType.مورد)
                return BadRequest("هذا التاجر ليس مورد.");

            // جلب الحركات من المخزن مع تفاصيل الأصناف
            var transactionsQuery = _context.WarehouseTransactions
                .Include(t => t.Item)
                .Include(t => t.Warehouse)
                .Where(t => t.TraderId == traderId && t.TransactionType == "Purchase")
                .OrderByDescending(t => t.Date);

            var transactions = await transactionsQuery
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .ToListAsync();

            // تجميع الأصناف حسب الفاتورة (TransactionId)
            var invoices = transactions
                .GroupBy(t => t.Id)
                .Select(g => new SupplierInvoiceDto
                {
                    TransactionId = g.Key,
                    Date = g.First().Date,
                    WarehouseName = g.First().Warehouse.Name,
                    PaidAmount = g.Sum(x => x.PaidAmount),
                    Items = g.Select(x => new SupplierInvoiceItemDto
                    {
                        ItemName = x.Item.Name,
                        Quantity = x.Quantity,
                        PricePerUnit = x.PricePerTon
                    }).ToList()
                })
                .ToList();

            // جلب الرصيد التراكمي
            var lastLedger = await _context.TraderLedgers
                .Where(l => l.TraderId == traderId)
                .OrderByDescending(l => l.Id)
                .FirstOrDefaultAsync();

            var report = new SupplierInvoiceReportDto
            {
                TraderName = trader.Name,
                CurrentBalance = lastLedger?.Balance ?? 0,
                Invoices = invoices
            };

            return Ok(report);
        }

        [HttpGet("customer/{traderId}/invoices")]
        public async Task<ActionResult<CustomerInvoiceReportDto>> GetCustomerInvoices(
            int traderId,
            int SkipCount = 0,
            int MaxResultCount = 10)
        {
            var trader = await _context.Traders.FindAsync(traderId);
            if (trader == null || trader.Type != TraderType.عميل)
                return BadRequest("هذا التاجر ليس عميلاً.");

            // جلب عمليات بيع البيض للعميل مع تفاصيل المخزن
            var salesQuery = _context.EggSales
                .Include(s => s.Warehouse)
                .Include(s => s.WarehouseTransactions)
                .ThenInclude(wt => wt.Item)
                .Where(s => s.TraderId == traderId)
                .OrderByDescending(s => s.Date);

            var sales = await salesQuery
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .ToListAsync();

            var invoices = sales.Select(s => new CustomerInvoiceDto
            {
                SaleId = s.Id,
                Date = s.Date,
                WarehouseName = s.Warehouse?.Name ?? "غير معروف",
                Items = s.WarehouseTransactions.Select(wt => new CustomerInvoiceItemDto
                {
                    ItemName = wt.Item.Name,
                    Quantity = wt.Quantity,
                    UnitPrice = s.UnitPrice,
                    TotalPrice = s.UnitPrice * wt.Quantity,
                    EggQuality = wt.EggQuality?.ToArabic() ?? "غير محدد" // سليم / كسر / دبل
                }).ToList(),
                TotalAmount = s.TotalPrice,
                PaidAmount = s.PaidAmount,
                RemainingAmount = s.RemainingAmount
            }).ToList();

            // رصيد تراكمي حالي للعميل
            var currentBalance = trader.Balance;

            var report = new CustomerInvoiceReportDto
            {
                TraderName = trader.Name,
                CurrentBalance = currentBalance,
                Invoices = invoices
            };

            return Ok(report);
        }
    }
}
