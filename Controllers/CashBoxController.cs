using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CashBoxController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public CashBoxController(FarmDbContext context)
        {
            _context = context;
        }

        //  عرض كل الحركات (الجديد فوق)
        [HttpGet]
        public async Task<IActionResult> GetAll(int SkipCount = 0, int MaxResultCount = 7) // القيمة الافتراضية 7
        {
            var query = _context.CashBoxTransactions
                .OrderByDescending(t => t.Id);

            var totalCount = await query.CountAsync();

            var transactions = await query
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Transactions = transactions
            });
        }

        //  تقرير الخزنة
        [HttpGet("report")]
        public async Task<IActionResult> GetReport(DateTime? from, DateTime? to)
        {
            var startDate = from ?? DateTime.MinValue;
            var endDate = to ?? DateTime.MaxValue;

            // 🔹 رصيد أول المدة
            var openingTransactions = await _context.CashBoxTransactions
                .Where(t => t.Date < startDate)
                .ToListAsync();

            var openingIncome = openingTransactions
                .Where(t => t.Type == "Income" || t.Type == "إيراد")
                .Sum(t => t.Amount);

            var openingExpense = openingTransactions
                .Where(t => t.Type == "Expense" || t.Type == "منصرف")
                .Sum(t => t.Amount);

            var openingBalance = openingIncome - openingExpense;

            // 🔹 حركات الفترة
            var periodTransactions = await _context.CashBoxTransactions
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .OrderByDescending(t => t.Id)
                .ToListAsync();

            var totalIncome = periodTransactions
                .Where(t => t.Type == "Income" || t.Type == "إيراد")
                .Sum(t => t.Amount);

            var totalExpense = periodTransactions
                .Where(t => t.Type == "Expense" || t.Type == "منصرف")
                .Sum(t => t.Amount);

            var closingBalance = openingBalance + totalIncome - totalExpense;

            return Ok(new
            {
                openingBalance,
                totalIncome,
                totalExpense,
                closingBalance,
                transactions = periodTransactions
            });
        }

        //  إضافة مصروف آخر
        [HttpPost("expense/other")]
        public async Task<IActionResult> AddOtherExpense([FromBody] CreateExpenseDto dto)
        {
            var date = dto.Date ?? DateTime.Now;

            var expense = new CashBoxTransaction
            {
                Date = date,
                Type = "منصرف",
                Category = "أخرى",
                Amount = dto.Amount,
                Notes = dto.Notes
            };

            _context.CashBoxTransactions.Add(expense);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تسجيل المصروف بنجاح", expense });
        }
//  إضافة إيراد آخر
[HttpPost("income/other")]
public async Task<IActionResult> AddOtherIncome([FromBody] CreateExpenseDto dto)
{
    var date = dto.Date ?? DateTime.Now;

    var income = new CashBoxTransaction
    {
        Date = date,
        Type = "إيراد",
        Category = "أخرى",
        Amount = dto.Amount,
        Notes = dto.Notes
    };

    _context.CashBoxTransactions.Add(income);
    await _context.SaveChangesAsync();

    return Ok(new { message = "تم تسجيل الإيراد بنجاح", income });
}
    }
}