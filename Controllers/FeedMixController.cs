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
public async Task<ActionResult<IEnumerable<FeedMixDto>>> GetFeedMixes()
{
    var feedMixes = await _context.FeedMixes
        .Include(fm => fm.FeedType)
        .Include(fm => fm.Details)
            .ThenInclude(d => d.Item)
        .OrderByDescending(fm => fm.Id) 
        .ToListAsync();

  
    var result = feedMixes.Select(fm => new FeedMixDto
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

    return Ok(result);
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
                    return BadRequest($"Not enough stock for ItemId {dtoItem.ItemId}.");

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
                TotalWeight = totalWeight,
                TotalPrice = totalPrice,
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
                    PricePerTon = totalPrice / totalWeight
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
    }
}
