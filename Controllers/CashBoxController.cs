using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;
using FarmManagement.API.Enums;

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
        public async Task<IActionResult> GetAll(int SkipCount = 0, int MaxResultCount = 7)
        {
            var query = _context.CashBoxTransactions
                .OrderByDescending(t => t.Id);

            var totalCount = await query.CountAsync();

            var transactions = await query
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .Select(t => new
                {
                    t.Id,
                    t.Date,

                    Type = EnumHelper.ToEnumResponse(t.Type),
                    Category = EnumHelper.ToEnumResponse(t.Category),

                    t.Amount,
                    t.Notes,
                    t.TraderId,
                    t.WorkerId,
                    t.WarehouseId
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Transactions = transactions
            });
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetReport(
    DateTime? from,
    DateTime? to,
    CashBoxCategory? category = null,
    CashBoxType? type = null,
    int SkipCount = 0,
    int MaxResultCount = 7)
        {
            var startDate = from ?? DateTime.MinValue;
            var endDate = to ?? DateTime.MaxValue;

            var baseQuery = _context.CashBoxTransactions
                .Where(t => t.Date >= startDate && t.Date <= endDate);

            // 🔹 فلترة بالـ Category
            if (category.HasValue)
                baseQuery = baseQuery.Where(t => t.Category == category.Value);

            // 🔹 فلترة بالـ Type
            if (type.HasValue)
                baseQuery = baseQuery.Where(t => t.Type == type.Value);

            var totalCount = await baseQuery.CountAsync();

            var transactions = await baseQuery
                .OrderByDescending(t => t.Id)
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .ToListAsync();

            var openingBalance = await _context.CashBoxTransactions
                .Where(t => t.Date < startDate)
                .Where(t => !category.HasValue || t.Category == category)
                .Where(t => !type.HasValue || t.Type == type)
                .SumAsync(t =>
                    t.Type == CashBoxType.Income ? t.Amount : -t.Amount
                );

            var totalIncome = await baseQuery
                .Where(t => t.Type == CashBoxType.Income)
                .SumAsync(t => t.Amount);

            var totalExpense = await baseQuery
                .Where(t => t.Type == CashBoxType.Expense)
                .SumAsync(t => t.Amount);

            var closingBalance = openingBalance + totalIncome - totalExpense;

            return Ok(new
            {
                openingBalance,
                totalIncome,
                totalExpense,
                closingBalance,
                totalCount,
                skipCount = SkipCount,
                maxResultCount = MaxResultCount,
                transactions = transactions.Select(t => new
                {
                    t.Id,
                    t.Date,
                    Type = EnumHelper.ToEnumResponse(t.Type),
                    Category = EnumHelper.ToEnumResponse(t.Category),
                    t.Amount,
                    t.Notes
                })
            });
        }

        [HttpPost("expense/other")]
        public async Task<IActionResult> AddOtherExpense([FromBody] CreateExpenseDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Notes))
                return BadRequest(new
                {
                    message = "يجب إدخال الوصف كملاحظة للمبلغ المصروف"
                });

            var date = dto.Date ?? DateTime.Now;

            var expense = new CashBoxTransaction
            {
                Date = date,
                Type = CashBoxType.Expense,
                Category = CashBoxCategory.Other,
                Amount = dto.Amount,
                Notes = dto.Notes
            };

            _context.CashBoxTransactions.Add(expense);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تسجيل المصروف بنجاح", expense });
        }

        //  إضافة إيراد آخر
        // [HttpPost("income/other")]
        // public async Task<IActionResult> AddOtherIncome([FromBody] CreateExpenseDto dto)
        // {
        //     var date = dto.Date ?? DateTime.Now;

        //     var income = new CashBoxTransaction
        //     {
        //         Date = date,
        //         Type = CashBoxType.Income,
        //         Category = CashBoxCategory.Other,
        //         Amount = dto.Amount,
        //         Notes = dto.Notes
        //     };

        //     _context.CashBoxTransactions.Add(income);
        //     await _context.SaveChangesAsync();

        //     return Ok(new { message = "تم تسجيل الإيراد بنجاح", income });
        // }
        [HttpPost("income/other")]
        public async Task<IActionResult> AddOtherIncome([FromBody] CreateExpenseDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Notes))
                return BadRequest(new
                {
                    message = "يجب إدخال الوصف كملاحظة للمبلغ الوارد"
                });

            var date = dto.Date ?? DateTime.Now;

            var income = new CashBoxTransaction
            {
                Date = date,
                Type = CashBoxType.Income,
                Category = CashBoxCategory.Other,
                Amount = dto.Amount,
                Notes = dto.Notes
            };

            _context.CashBoxTransactions.Add(income);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تسجيل الإيراد بنجاح", income });
        }
    }
}