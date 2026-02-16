using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public WarehouseController(FarmDbContext context)
        {
            _context = context;
        }


        // CRUD Warehouses
  

        [HttpPost]
        public async Task<ActionResult> CreateWarehouse(WarehouseCreateDto dto)
        {
            var farm = await _context.Farms.FindAsync(dto.FarmId);
            if (farm == null) return BadRequest("المزرعة غير موجودة");

            var exists = await _context.Warehouses.AnyAsync(w => w.FarmId == dto.FarmId);
            if (exists) return BadRequest("هذه المزرعة تمتلك مخزناً بالفعل.");

            var warehouse = new Warehouse
            {
                Name = dto.Name,
                FarmId = dto.FarmId
            };

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم إنشاء المخزن بنجاح", warehouse.Id, warehouse.Name });
        }

        [HttpGet]
        public async Task<ActionResult> GetWarehouses()
        {
            var warehouses = await _context.Warehouses
                .Include(w => w.Farm)
                .Select(w => new { w.Id, w.Name, FarmName = w.Farm.Name })
                .ToListAsync();

            return Ok(warehouses);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateWarehouse(int id, WarehouseUpdateDto dto)
{
           var warehouse = await _context.Warehouses.FindAsync(id);
           if (warehouse == null) return NotFound("المخزن غير موجود");

            warehouse.Name = dto.Name;

           await _context.SaveChangesAsync();

           return Ok(new { message = "تم تحديث المخزن بنجاح", warehouse.Id, warehouse.Name });
}


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWarehouse(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null) return NotFound("المخزن غير موجود");

            if (await _context.WarehouseItems.AnyAsync(wi => wi.WarehouseId == id))
                return BadRequest("لا يمكن حذف المخزن لأنه يحتوي على أصناف.");

            if (await _context.WarehouseTransactions.AnyAsync(t => t.WarehouseId == id))
                return BadRequest("لا يمكن حذف المخزن لوجود حركات مسجلة عليه.");

            _context.Warehouses.Remove(warehouse);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم حذف المخزن بنجاح" });
        }

        // Warehouse Transactions (Purchases Only)
     

        [HttpPost("transaction")]
        public async Task<ActionResult> AddWarehouseTransaction(WarehouseTransactionCreateDto dto)
        {
            var trader = await _context.Traders.FindAsync(dto.TraderId);
            if (trader == null || trader.Type != TraderType.مورد)
                return BadRequest("يجب اختيار تاجر من نوع (مورد Supplier) لتنفيذ هذه العملية.");

            var date = dto.Date ?? DateTime.Now;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var itemDto in dto.Items)
                {
                    
                    var warehouseTrans = new WarehouseTransaction
                    {
                        WarehouseId = dto.WarehouseId,
                        TraderId = dto.TraderId,
                        ItemId = itemDto.ItemId,
                        Quantity = itemDto.Quantity,
                        PricePerTon = itemDto.PricePerTon,
                        TotalPrice = itemDto.Quantity * itemDto.PricePerTon,
                        Date = date,
                        TransactionType = "Purchase" 
                    };
                    _context.WarehouseTransactions.Add(warehouseTrans);

                    var warehouseItem = await _context.WarehouseItems
                        .FirstOrDefaultAsync(wi => wi.WarehouseId == dto.WarehouseId && wi.ItemId == itemDto.ItemId);

                    if (warehouseItem == null)
                    {
                        _context.WarehouseItems.Add(new WarehouseItem
                        {
                            WarehouseId = dto.WarehouseId,
                            ItemId = itemDto.ItemId,
                            Quantity = itemDto.Quantity,
                            PricePerUnit = itemDto.PricePerTon,
                            Withdrawn = 0
                        });
                    }
                    else
                    {
                        warehouseItem.Quantity += itemDto.Quantity;
                        warehouseItem.PricePerUnit = itemDto.PricePerTon; 
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "تم تسجيل التوريد وتحديث المخزن بنجاح" });
            }
            catch {
                await transaction.RollbackAsync();
                return BadRequest("حدث خطأ أثناء حفظ البيانات");
            }
        }

        [HttpDelete("transaction/{id}")]
        public async Task<ActionResult> DeleteTransaction(int id)
        {
            var trans = await _context.WarehouseTransactions.FindAsync(id);
            if (trans == null) return NotFound("الحركة غير موجودة");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var warehouseItem = await _context.WarehouseItems
                    .FirstOrDefaultAsync(wi => wi.WarehouseId == trans.WarehouseId && wi.ItemId == trans.ItemId);

                if (warehouseItem != null)
                {
                    if (warehouseItem.Quantity < trans.Quantity)
                        return BadRequest("لا يمكن حذف الحركة لأن الكمية المستخدمة أكبر من الرصيد المتبقي.");

                    warehouseItem.Quantity -= trans.Quantity;
                }

                _context.WarehouseTransactions.Remove(trans);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "تم حذف الحركة وتعديل الرصيد" });
            }
            catch {
                await transaction.RollbackAsync();
                return BadRequest("فشل حذف الحركة");
            }
        }

    
        // Lookups & Reports
       

        [HttpGet("{warehouseId}/items")]
        public async Task<ActionResult<IEnumerable<WarehouseItemDto>>> GetWarehouseItems(int warehouseId)
        {
            var items = await _context.WarehouseItems
                .Include(wi => wi.Item)
                .Include(wi => wi.Warehouse)
                .Where(wi => wi.WarehouseId == warehouseId)
                .Select(wi => new WarehouseItemDto
                {
                    Id = wi.Id,
                    WarehouseId = wi.WarehouseId,
                    WarehouseName = wi.Warehouse.Name,
                    ItemId = wi.ItemId,
                    ItemName = wi.Item.Name,
                    Quantity = wi.Quantity,
                    PricePerUnit = wi.PricePerUnit,
                    Withdrawn = wi.Withdrawn
                })
                .ToListAsync();

            return Ok(items);
        }
    }
}