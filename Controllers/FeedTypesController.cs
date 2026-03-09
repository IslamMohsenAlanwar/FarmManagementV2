using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedTypesController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public FeedTypesController(FarmDbContext context)
        {
            _context = context;
        }

        // GET: api/FeedTypes
        [HttpGet]
        [HttpGet]
        public async Task<ActionResult> GetFeedTypes(
    int SkipCount = 0,
    int MaxResultCount = 7) 
        {
            var query = _context.FeedTypes
                .OrderBy(t => t.Id); 

            var totalCount = await query.CountAsync();

            var types = await query
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .Select(t => new
                {
                    t.Id,
                    t.Name
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                FeedTypes = types
            });
        }
    }
}
