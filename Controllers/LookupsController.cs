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

    [HttpGet("feed-mixes")]
    public async Task<ActionResult<IEnumerable<LookupDto>>> GetFeedMixes()
    {
        var mixes = await _context.Items
            .Where(i => i.ItemType == ItemType.FeedMix)
            .Select(i => new LookupDto
            {
                Id = i.Id,
                Name = i.Name
            })
            .ToListAsync();

        return Ok(mixes);
    }

    [HttpGet("medicines")]
    public async Task<ActionResult<IEnumerable<LookupDto>>> GetMedicines()
    {
        var medicines = await _context.Items
            .Where(i => i.ItemType == ItemType.Medicine)
            .Select(i => new LookupDto
            {
                Id = i.Id,
                Name = i.Name
            })
            .ToListAsync();

        return Ok(medicines);
    }

    [HttpGet("store-items")]
    public async Task<ActionResult<IEnumerable<LookupDto>>> GetStoreItems()
    {
        var items = await _context.Items
            .Where(i => i.ItemType == ItemType.RawMaterial 
                     || i.ItemType == ItemType.Medicine)
            .Select(i => new LookupDto
            {
                Id = i.Id,
                Name = i.Name
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("eggs")]
    public async Task<ActionResult<IEnumerable<LookupDto>>> GetEggItems()
    {
        var eggs = await _context.Items
            .Where(i => i.ItemType == ItemType.Egg)
            .Select(i => new LookupDto
            {
                Id = i.Id,
                Name = i.Name
            })
            .ToListAsync();

        return Ok(eggs);
    }
        // ================= GET: Active Cycle =================
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

            return result;
        }

        // ================= GET: Barn Lookup =================
        [HttpGet("barns/{farmId}")]
public async Task<ActionResult<IEnumerable<BarnLookupDto>>> GetBarnsByFarm(int farmId)
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

    return result;
}

// ================= GET Suppliers =================
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

// ================= GET Buyers =================
[HttpGet("buyers")]
public async Task<ActionResult<IEnumerable<TraderDto>>> GetBuyers()
{
    var buyers = await _context.Traders
        .Where(t => t.Type == TraderType.مشتري) 
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


}

}