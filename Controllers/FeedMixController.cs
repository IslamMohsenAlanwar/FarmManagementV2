using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedMixController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public FeedMixController(FarmDbContext context)
        {
            _context = context;
        }

        // GET: api/feedmix

        [HttpGet]
        public async Task<ActionResult> GetFeedMixes(
    int SkipCount = 0,
    int MaxResultCount = 7) // القيمة الافتراضية 7
        {
            var query = _context.FeedMixes
                .Include(fm => fm.FeedType)
                .Include(fm => fm.Details)
                    .ThenInclude(d => d.Item)
                .OrderByDescending(fm => fm.Id);

            var totalCount = await query.CountAsync();

            var feedMixesList = await query
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .ToListAsync();

            var result = feedMixesList.Select(fm => new FeedMixDto
            {
                Id = fm.Id,
                Name = $"{fm.FeedType.Name} - خلطة جاهزة",
                TotalWeight = fm.TotalWeight,
                TotalPrice = fm.TotalPrice,
                Items = fm.Details.Select(d => new FeedMixDetailDto
                {
                    ItemId = d.ItemId,
                    ItemName = d.Item.Name,
                    Quantity = d.Quantity
                }).ToList()
            }).ToList();

            return Ok(new
            {
                TotalCount = totalCount,
                FeedMixes = result
            });
        }


        // POST: api/feedmix
        [HttpPost]
        public async Task<ActionResult<FeedMixDto>> CreateFeedMix(FeedMixCreateDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                return BadRequest("FeedMix must have at least one item.");

            var feedType = await _context.FeedTypes.FindAsync(dto.FeedTypeId);
            if (feedType == null)
                return BadRequest("Invalid Feed Type");

            decimal totalWeight = 0;
            decimal totalPrice = 0;
            var details = new List<FeedMixDetail>();

            foreach (var dtoItem in dto.Items)
            {
                var warehouseItem = await _context.WarehouseItems
                    .Include(wi => wi.Item)
                    .FirstOrDefaultAsync(wi =>
                        wi.WarehouseId == dto.WarehouseId &&
                        wi.ItemId == dtoItem.ItemId);

                if (warehouseItem == null)
                    return BadRequest($"ItemId {dtoItem.ItemId} not found in warehouse.");

                if (warehouseItem.Quantity < dtoItem.Quantity)
                    return BadRequest($"لا يوجد مخزون كاف من {warehouseItem.Item.Name}.");

                warehouseItem.Quantity -= dtoItem.Quantity;
                warehouseItem.Withdrawn += dtoItem.Quantity;

                var itemPrice = dtoItem.Quantity * warehouseItem.PricePerUnit;
                totalWeight += dtoItem.Quantity;
                totalPrice += itemPrice;

                details.Add(new FeedMixDetail
                {
                    ItemId = dtoItem.ItemId,
                    Quantity = dtoItem.Quantity,
                    Price = itemPrice
                });
            }

            var feedMix = new FeedMix
            {
                FeedTypeId = dto.FeedTypeId,
                WarehouseId = dto.WarehouseId,  // ← ضيف ده
                TotalWeight = totalWeight,
                TotalPrice = totalPrice,
                Quantity = totalWeight,
                Details = details
            };
            _context.FeedMixes.Add(feedMix);

            var warehouseItemName = $"{feedType.Name} - خلطة جاهزة";

            var existingWarehouseItem = await _context.WarehouseItems
                .Include(wi => wi.Item)
                .FirstOrDefaultAsync(wi => wi.WarehouseId == dto.WarehouseId &&
                                           wi.Item.Name == warehouseItemName);

            if (existingWarehouseItem == null)
            {
                var feedMixItem = new Item
                {
                    Name = warehouseItemName,
                    ItemType = ItemType.FeedMix,
                    PricePerTon = totalPrice / totalWeight,
                    FeedTypeId = dto.FeedTypeId
                    
                };
                _context.Items.Add(feedMixItem);
                await _context.SaveChangesAsync(); 

                var warehouseFeedMixItem = new WarehouseItem
                {
                    WarehouseId = dto.WarehouseId,
                    ItemId = feedMixItem.Id,
                    Quantity = totalWeight,
                    PricePerUnit = totalPrice / totalWeight,
                    Withdrawn = 0
                };
                _context.WarehouseItems.Add(warehouseFeedMixItem);
            }
            else
            {
                var existingTotalValue = existingWarehouseItem.Quantity * existingWarehouseItem.PricePerUnit;
                var newTotalValue = existingTotalValue + totalPrice;
                existingWarehouseItem.Quantity += totalWeight;
                existingWarehouseItem.PricePerUnit = newTotalValue / existingWarehouseItem.Quantity;
            }

            await _context.SaveChangesAsync();

            var feedMixDto = new FeedMixDto
            {
                Id = feedMix.Id,
                Name = warehouseItemName,
                TotalWeight = totalWeight,
                TotalPrice = totalPrice,
                Items = details.Select(d => new FeedMixDetailDto
                {
                    ItemId = d.ItemId,
                    ItemName = _context.Items.First(i => i.Id == d.ItemId).Name,
                    Quantity = d.Quantity
                }).ToList()
            };

            return Ok(feedMixDto);
        }


 [HttpDelete("{id}")]
public async Task<ActionResult> DeleteFeedMix(int id)
{
    var feedMix = await _context.FeedMixes
        .Include(fm => fm.FeedType)
        .Include(fm => fm.Details)
            .ThenInclude(d => d.Item)
        .FirstOrDefaultAsync(fm => fm.Id == id);

    if (feedMix == null)
        return NotFound("FeedMix not found.");

    // ✅ check صح
    if (feedMix.Quantity < feedMix.TotalWeight)
        return BadRequest("لا يمكن حذف الخلطة لأنه تم استخدام جزء منها.");

    var warehouseItemName = $"{feedMix.FeedType.Name} - خلطة جاهزة";

    // ✅ فلتر بالـ WarehouseId
    var feedMixWarehouseItem = await _context.WarehouseItems
        .Include(wi => wi.Item)
        .FirstOrDefaultAsync(wi => wi.WarehouseId == feedMix.WarehouseId &&
                                   wi.Item.Name == warehouseItemName);

    // ✅ رجّع الكميات للمواد الأصلية بالـ WarehouseId الصح
    foreach (var detail in feedMix.Details)
    {
        var warehouseItem = await _context.WarehouseItems
            .FirstOrDefaultAsync(wi => wi.ItemId == detail.ItemId &&
                                       wi.WarehouseId == feedMix.WarehouseId);
        if (warehouseItem != null)
        {
            warehouseItem.Quantity += detail.Quantity;
            warehouseItem.Withdrawn -= detail.Quantity;
        }
    }

    // شيل الخلطة من الوير هوس
    if (feedMixWarehouseItem != null)
    {
        feedMixWarehouseItem.Quantity -= feedMix.TotalWeight;

        if (feedMixWarehouseItem.Quantity <= 0)
        {
            _context.WarehouseItems.Remove(feedMixWarehouseItem);
            _context.Items.Remove(feedMixWarehouseItem.Item);
        }
    }

    _context.FeedMixes.Remove(feedMix);
    await _context.SaveChangesAsync();

    return Ok("FeedMix deleted and quantities restored successfully.");
}
    }
}
