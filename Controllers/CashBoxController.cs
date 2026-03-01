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

        // ✅ عرض كل الحركات
[HttpGet]
public async Task<IActionResult> GetAll()
{
    var transactions = await _context.CashBoxTransactions
        .OrderByDescending(t => t.Id) 
        .ToListAsync();

    return Ok(transactions);
}

        // ✅ تقرير الخزنة
[HttpGet("report")]
public async Task<IActionResult> GetReport(DateTime? from, DateTime? to)
{
    var startDate = from ?? DateTime.MinValue;
    var endDate = to ?? DateTime.MaxValue;

    var openingTransactions = await _context.CashBoxTransactions
        .Where(t => t.Date < startDate)
        .ToListAsync();

    var openingIncome = openingTransactions
        .Where(t => t.Type == "Income")
        .Sum(t => t.Amount);

    var openingExpense = openingTransactions
        .Where(t => t.Type == "Expense" || t.Type == "منصرف")
        .Sum(t => t.Amount);

    var openingBalance = openingIncome - openingExpense;

    var periodTransactions = await _context.CashBoxTransactions
        .Where(t => t.Date >= startDate && t.Date <= endDate)
        .OrderByDescending(t => t.Id) 
        .ToListAsync();

    var totalIncome = periodTransactions
        .Where(t => t.Type == "Income")
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
    }
}