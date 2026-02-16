using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetWarehouseController : ControllerBase
    {
        private readonly FarmDbContext _context;
        public AssetWarehouseController(FarmDbContext context) => _context = context;

        // ================== Post CreateWarehouse ==================
        [HttpPost("create")]
        public async Task<IActionResult> CreateWarehouse(CreateAssetWarehouseDto dto)
        {
            var farm = await _context.Farms.FindAsync(dto.FarmId);
            if (farm == null) return BadRequest("Farm not found.");

            var existing = await _context.AssetWarehouses
                .FirstOrDefaultAsync(w => w.FarmId == dto.FarmId && w.Name == dto.Name);
            if (existing != null) return BadRequest("Asset warehouse with this name already exists for this farm.");

            var warehouse = new AssetWarehouse
            {
                FarmId = dto.FarmId,
                Name = dto.Name
            };

            _context.AssetWarehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                WarehouseId = warehouse.Id,
                FarmId = warehouse.FarmId,
                WarehouseName = warehouse.Name
            });
        }

        // ================== Get Warehouse ==================
        [HttpGet("{farmId}")]
        public async Task<ActionResult<AssetWarehouseDto>> GetWarehouse(int farmId)
        {
            var warehouse = await _context.AssetWarehouses
                .Include(w => w.Items)
                    .ThenInclude(i => i.AssetItem)
                .Include(w => w.Farm)
                .FirstOrDefaultAsync(w => w.FarmId == farmId);

            if (warehouse == null) return NotFound("Asset warehouse not found.");

            var dto = new AssetWarehouseDto
            {
                Id = warehouse.Id,
                FarmId = warehouse.FarmId,
                FarmName = warehouse.Farm?.Name ?? "",
                Name = warehouse.Name,
                Items = warehouse.Items
                .OrderByDescending(i => i.Id)
                .Select(i => new AssetWarehouseItemDto
                {
                    Id = i.Id,
                    AssetItemId = i.AssetItemId,
                    AssetItemName = i.AssetItem?.Name ?? "",
                    Quantity = i.Quantity,
                    InBarnsQuantity = i.InBarnsQuantity,
                    UnitPrice = i.UnitPrice,
                }).ToList()
            };

            return Ok(dto);
        }

        // ==================Post AddAssetToWarehouse==================
        [HttpPost("add")]
        public async Task<IActionResult> AddToWarehouse(AddAssetToWarehouseDto dto)
        {
            var warehouse = await _context.AssetWarehouses.FindAsync(dto.AssetWarehouseId);
            if (warehouse == null) return BadRequest("Asset warehouse not found.");

            var assetItem = await _context.AssetItems.FindAsync(dto.AssetItemId);
            if (assetItem == null) return BadRequest("Asset item not found.");

            var existing = await _context.AssetWarehouseItems
                .FirstOrDefaultAsync(i => i.AssetWarehouseId == warehouse.Id && i.AssetItemId == dto.AssetItemId);

            if (existing == null)
            {
                existing = new AssetWarehouseItem
                {
                    AssetWarehouseId = warehouse.Id,
                    AssetItemId = dto.AssetItemId,
                    Quantity = dto.Quantity,
                    UnitPrice = dto.UnitPrice,
                    InBarnsQuantity = 0
                };
                _context.AssetWarehouseItems.Add(existing);
            }
            else
            {
                existing.Quantity += dto.Quantity;
                existing.UnitPrice = dto.UnitPrice;
                _context.AssetWarehouseItems.Update(existing);
            }

            await _context.SaveChangesAsync();
            return Ok("Asset added to warehouse successfully.");
        }

        // ================== Post WithdrawAssetToBarn ==================
        [HttpPost("withdraw")]
        public async Task<IActionResult> WithdrawAsset(AssetTransactionDto dto)
        {
            var warehouseItem = await _context.AssetWarehouseItems
                .Include(i => i.AssetItem)
                .FirstOrDefaultAsync(i => i.Id == dto.AssetWarehouseItemId);

            if (warehouseItem == null) return BadRequest("Asset not found in warehouse.");
            if (dto.Quantity <= 0) return BadRequest("Quantity must be greater than zero.");
            if (dto.Quantity > warehouseItem.Quantity) return BadRequest("Not enough quantity in warehouse.");

            warehouseItem.Quantity -= dto.Quantity;
            warehouseItem.InBarnsQuantity += dto.Quantity;
            _context.AssetWarehouseItems.Update(warehouseItem);

            var transaction = new AssetTransaction
            {
                AssetWarehouseItemId = warehouseItem.Id,
                TargetBarnId = dto.BarnId,
                Quantity = dto.Quantity,
                TransactionType = "Withdraw",
                Date = dto.Date
            };

            _context.AssetTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Asset withdrawn successfully.",
                WarehouseItemId = warehouseItem.Id,
                RemainingQuantity = warehouseItem.Quantity,
                InBarnsQuantity = warehouseItem.InBarnsQuantity,
                TransactionId = transaction.Id
            });
        }

        // ================== Post DepositAssetToWarehouse ==================
        [HttpPost("deposit")]
        public async Task<IActionResult> DepositAsset(AssetTransactionDto dto)
        {
            var warehouseItem = await _context.AssetWarehouseItems
                .Include(i => i.AssetItem)
                .FirstOrDefaultAsync(i => i.Id == dto.AssetWarehouseItemId);

            if (warehouseItem == null) return BadRequest("Asset not found in warehouse.");
            if (dto.Quantity <= 0) return BadRequest("Quantity must be greater than zero.");
            if (dto.Quantity > warehouseItem.InBarnsQuantity) return BadRequest("Cannot deposit more than withdrawn quantity.");

            warehouseItem.Quantity += dto.Quantity;
            warehouseItem.InBarnsQuantity -= dto.Quantity;
            _context.AssetWarehouseItems.Update(warehouseItem);

            var transaction = new AssetTransaction
            {
                AssetWarehouseItemId = warehouseItem.Id,
                TargetBarnId = dto.BarnId,
                Quantity = dto.Quantity,
                TransactionType = "Deposit",
                Date = dto.Date
            };

            _context.AssetTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Asset deposited successfully.",
                WarehouseItemId = warehouseItem.Id,
                RemainingQuantity = warehouseItem.Quantity,
                InBarnsQuantity = warehouseItem.InBarnsQuantity,
                TransactionId = transaction.Id
            });
        }

        // ================== Get TransactionsHistory==================
        [HttpGet("transactions/{farmId}")]
        public async Task<IActionResult> GetTransactions(int farmId)
        {
            var warehouse = await _context.AssetWarehouses
                .Include(w => w.Farm)
                .FirstOrDefaultAsync(w => w.FarmId == farmId);

            if (warehouse == null) return NotFound("Asset warehouse not found.");

            // ===== Get From DB =====
            var transactionsList = await _context.AssetTransactions
                .Include(t => t.AssetWarehouseItem)
                    .ThenInclude(i => i.AssetItem)
                .Include(t => t.TargetBarn)
                .Where(t => t.AssetWarehouseItem.AssetWarehouseId == warehouse.Id)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            // =====  DTO =====
            var transactions = transactionsList.Select(t => new AssetTransactionResponseDto
            {
                Id = t.Id,
                AssetWarehouseItemId = t.AssetWarehouseItemId,
                AssetItemId = t.AssetWarehouseItem.AssetItemId,
                AssetItemName = t.AssetWarehouseItem.AssetItem?.Name ?? "",
                Quantity = t.Quantity,
                InBarnsQuantity = t.AssetWarehouseItem.InBarnsQuantity,
                UnitPrice = t.AssetWarehouseItem.UnitPrice,
                TotalValue = t.AssetWarehouseItem.Quantity * t.AssetWarehouseItem.UnitPrice,
                WarehouseId = t.AssetWarehouseItem.AssetWarehouseId,
                WarehouseName = warehouse.Name,
                FarmId = warehouse.FarmId,
                FarmName = warehouse.Farm?.Name ?? "",
                BarnId = t.TargetBarnId,
                BarnName = t.TargetBarn?.Name ?? "",
                TransactionType = t.TransactionType,
                Date = t.Date
            });

            return Ok(transactions);
        }
    }
}
