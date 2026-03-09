using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [ApiController]
    [Route("api/lookups")]
    public class LookupsController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public LookupsController(FarmDbContext context)
        {
            _context = context;
        }

        // ================== Items ==================
        [HttpGet("feed-mixes")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetFeedMixes()
        {
            var mixes = await _context.Items
                .Where(i => i.ItemType == ItemType.FeedMix)
                .Select(i => new LookupDto { Id = i.Id, Name = i.Name })
                .ToListAsync();

            return Ok(mixes);
        }

        [HttpGet("medicines")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetMedicines()
        {
            var medicines = await _context.Items
                .Where(i => i.ItemType == ItemType.Medicine)
                .Select(i => new LookupDto { Id = i.Id, Name = i.Name })
                .ToListAsync();

            return Ok(medicines);
        }

        [HttpGet("store-items")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetStoreItems()
        {
            var items = await _context.Items
                .Where(i => i.ItemType == ItemType.RawMaterial || i.ItemType == ItemType.Medicine)
                .Select(i => new LookupDto { Id = i.Id, Name = i.Name })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("eggs")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetEggItems()
        {
            var eggs = await _context.Items
                .Where(i => i.ItemType == ItemType.Egg)
                .Select(i => new LookupDto { Id = i.Id, Name = i.Name })
                .ToListAsync();

            return Ok(eggs);
        }

        // ================== Active Cycles ==================
        [HttpGet("active-cycles/{farmId}")]
        public async Task<ActionResult<IEnumerable<ActiveCycleDto>>> GetActiveCycles(int farmId)
        {
            var today = DateTime.Now.Date;

            var activeCycles = await _context.Cycles
                .Include(c => c.Barn)
                .Where(c => c.FarmId == farmId &&
                            c.StartDate.Date <= today &&
                            c.EndDate.Date >= today)
                .OrderBy(c => c.StartDate)
                .ToListAsync();

            var result = activeCycles.Select(c => new ActiveCycleDto
            {
                Id = c.Id,
                CycleName = c.Name,
                BarnName = c.Barn.Name
            }).ToList();

            return Ok(result);
        }

        // ================== Barns Lookup ==================
        [HttpGet("barns/{farmId}")]
        public async Task<ActionResult<IEnumerable<BarnLookupDto>>> GetBarnsLookup(int farmId)
        {
            var barns = await _context.Barns
                .Where(b => b.FarmId == farmId)
                .OrderBy(b => b.Name)
                .ToListAsync();

            var result = barns.Select(b => new BarnLookupDto
            {
                Id = b.Id,
                Name = b.Name
            }).ToList();

            return Ok(result);
        }

        // ================== Traders ==================
        [HttpGet("suppliers")]
        public async Task<ActionResult<IEnumerable<TraderDto>>> GetSuppliers()
        {
            var suppliers = await _context.Traders
                .Where(t => t.Type == TraderType.مورد)
                .OrderBy(t => t.Name)
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

            return Ok(suppliers);
        }

        [HttpGet("buyers")]
        public async Task<ActionResult<IEnumerable<TraderDto>>> GetBuyers()
        {
            var buyers = await _context.Traders
                .Where(t => t.Type == TraderType.عميل)
                .OrderBy(t => t.Name)
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

            return Ok(buyers);
        }

        // ================== Upcoming Cycles ==================
        [HttpGet("upcoming-cycles")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetUpcomingCycles()
        {
            var today = DateTime.Today;

            var cycles = await _context.Cycles
                .Where(c => c.EndDate >= today)
                .OrderBy(c => c.EndDate)
                .Select(c => new LookupDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            return Ok(cycles);
        }

        // ================== Warehouse Lookup ==================
        [HttpGet("warehouse/by-farm/{farmId}")]
        public async Task<ActionResult<WarehouseDto>> GetWarehouseByFarm(int farmId)
        {
            var warehouse = await _context.Warehouses
                .Include(w => w.Farm)
                .Where(w => w.FarmId == farmId)
                .Select(w => new WarehouseDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    FarmId = w.FarmId,
                    FarmName = w.Farm.Name
                })
                .FirstOrDefaultAsync();

            if (warehouse == null)
                return NotFound("لا يوجد مخزن لهذه المزرعة");

            return Ok(warehouse);
        }

        // ================== Farms Lookup ==================
        [HttpGet("farms")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetFarmsLookup()
        {
            var farms = await _context.Farms
                .OrderBy(f => f.Name)
                .Select(f => new LookupDto
                {
                    Id = f.Id,
                    Name = f.Name
                })
                .ToListAsync();

            return Ok(farms);
        }

        // ================== Evaluation Items ==================
        [HttpGet("evaluation-items")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetEvaluationItems()
        {
            var items = await _context.EvaluationItems
                .OrderBy(e => e.Name)
                .Select(e => new LookupDto { Id = e.Id, Name = e.Name })
                .ToListAsync();

            return Ok(items);
        }

        // ================== Asset Items ==================
        [HttpGet("asset-items")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetAssetItems()
        {
            var items = await _context.AssetItems
                .OrderBy(a => a.Name)
                .Select(a => new LookupDto { Id = a.Id, Name = a.Name })
                .ToListAsync();

            return Ok(items);
        }
    }
}