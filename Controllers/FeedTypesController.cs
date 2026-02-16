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
        public async Task<ActionResult> GetFeedTypes()
        {
            var types = await _context.FeedTypes
                .Select(t => new
                {
                    t.Id,
                    t.Name
                })
                .ToListAsync();

            return Ok(types);
        }
    }
}
