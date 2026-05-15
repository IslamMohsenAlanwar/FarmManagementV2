using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetItemsController : ControllerBase
    {
        private readonly FarmDbContext _context;
        public AssetItemsController(FarmDbContext context) => _context = context;

        // ================= GET =================
        [HttpGet]
        public async Task<ActionResult> GetAll(int SkipCount = 0, int MaxResultCount = 7)
        {
            var totalCount = await _context.AssetItems.CountAsync();

            var items = await _context.AssetItems
                .OrderByDescending(a => a.Id)
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .Select(a => new AssetItemDto
                {
                    Id = a.Id,
                    Name = a.Name
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Items = items
            });
        }

        // ================= POST =================
        [HttpPost]
        public async Task<ActionResult<AssetItemDto>> Create(CreateAssetItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name cannot be empty.");

            var name = dto.Name.Trim();

            // (Case Insensitive)
            var exists = await _context.AssetItems
                .AnyAsync(a => a.Name.ToLower() == name.ToLower());

            if (exists)
                return BadRequest("Asset name already exists.");

            var item = new AssetItem
            {
                Name = name
            };

            _context.AssetItems.Add(item);
            await _context.SaveChangesAsync();

            var result = new AssetItemDto
            {
                Id = item.Id,
                Name = item.Name
            };

            return CreatedAtAction(nameof(GetAll), new { id = item.Id }, result);
        }

        // ================= PUT =================
        [HttpPut("{id}")]
        public async Task<ActionResult<AssetItemDto>> Update(int id, UpdateAssetItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name cannot be empty.");

            var item = await _context.AssetItems.FindAsync(id);

            if (item == null)
                return NotFound("Asset item not found.");

            var name = dto.Name.Trim();

            var exists = await _context.AssetItems
                .AnyAsync(a => a.Name.ToLower() == name.ToLower() && a.Id != id);

            if (exists)
                return BadRequest("Asset name already exists.");

            item.Name = name;

            await _context.SaveChangesAsync();

            return Ok(new AssetItemDto
            {
                Id = item.Id,
                Name = item.Name
            });
        }

         // ================= DELETE =================
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(
    int id,
    [FromQuery] bool forceDelete = false)
{
    var item = await _context.AssetItems
        .FirstOrDefaultAsync(a => a.Id == id);

    if (item == null)
        return NotFound("Asset item not found.");

    var warehouseItems = await _context.AssetWarehouseItems
        .Where(w => w.AssetItemId == id)
        .ToListAsync();

    var warehouseItemIds = warehouseItems
        .Select(w => w.Id)
        .ToList();

    var transactions = await _context.AssetTransactions
        .Where(t => warehouseItemIds.Contains(t.AssetWarehouseItemId))
        .ToListAsync();

    // 🚨 تحذير قبل المسح
    if (!forceDelete &&
        (warehouseItems.Any() || transactions.Any()))
    {
        return BadRequest(new
        {
            Message = "الأصل عليه حركات ومخزون، هل متأكد من حذفه؟",
            HasWarehouseItems = warehouseItems.Any(),
            WarehouseItemsCount = warehouseItems.Count,
            HasTransactions = transactions.Any(),
            TransactionsCount = transactions.Count,
            RequireConfirmation = true
        });
    }

    // 1️⃣ امسح الـ Transactions
    _context.AssetTransactions.RemoveRange(transactions);

    // 2️⃣ امسح الـ WarehouseItems
    _context.AssetWarehouseItems.RemoveRange(warehouseItems);

    // 3️⃣ امسح الـ AssetItem
    _context.AssetItems.Remove(item);

    await _context.SaveChangesAsync();

    return Ok(new
    {
        Message = "تم حذف الأصل والبيانات ذات الصلة بنجاح."
    });
}
    }
}
