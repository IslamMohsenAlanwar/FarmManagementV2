using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseLookupsController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public WarehouseLookupsController(FarmDbContext context)
        {
            _context = context;
        }

        // GET api/WarehouseLookups/{warehouseId}/summary
        [HttpGet("{warehouseId}/summary")]
        public async Task<ActionResult<WarehouseStockSummaryDto>> GetWarehouseStockSummary(int warehouseId)
        {
            var warehouse = await _context.Warehouses.FindAsync(warehouseId);
            if (warehouse == null) return NotFound("Warehouse not found");

            var items = await _context.WarehouseItems
                .Where(wi => wi.WarehouseId == warehouseId)
                .ToListAsync();

            var dto = new WarehouseStockSummaryDto
            {
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.Name,
                TotalQuantity = items.Sum(i => i.Quantity),
                TotalStockValue = items.Sum(i => i.Quantity * i.PricePerUnit),
                RemainingQuantity = items.Sum(i => i.Quantity - i.Withdrawn),
                RemainingStockValue = items.Sum(i => (i.Quantity - i.Withdrawn) * i.PricePerUnit)
            };

            return Ok(dto);
        }
    }
}
