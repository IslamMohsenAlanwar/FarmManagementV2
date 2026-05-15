using Microsoft.AspNetCore.Mvc;
using FarmManagement.API.Data;
using FarmManagement.API.Services;
using Microsoft.EntityFrameworkCore;

namespace FarmManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BonusController : ControllerBase
    {
        private readonly FarmDbContext _context;
        private readonly BonusService _bonusService;

        public BonusController(FarmDbContext context, BonusService bonusService)
        {
            _context = context;
            _bonusService = bonusService;
        }

        [HttpGet("{workerId}")]
        public async Task<IActionResult> GetBonus(int workerId)
        {
            var worker = await _context.Workers.FindAsync(workerId);
            if (worker == null)
                return NotFound("Worker not found");

            var achievement = await _bonusService.GetWorkerAchievement(worker.Id, worker.Role);

            var percent = _bonusService.GetBonusPercentage(achievement);

            var bonusAmount = worker.Salary * percent;

            return Ok(new
            {
                worker.Name,
                worker.Role,
                worker.Salary,

                Achievement = Math.Round(achievement, 2),

                BonusPercent = percent * 100,
                BonusAmount = Math.Round(bonusAmount, 2),

                TotalSalary = worker.Salary + bonusAmount
            });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllBonuses(
    [FromQuery] int SkipCount = 0,
    [FromQuery] int MaxResultCount = 7)
        {
            var workersQuery = _context.Workers
                .OrderBy(w => w.Id);

            var totalCount = await workersQuery.CountAsync();

            var workers = await workersQuery
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .ToListAsync();

            var result = new List<object>();

            foreach (var worker in workers)
            {
                var achievement = await _bonusService.GetWorkerAchievement(worker.Id, worker.Role);

                var bonusPercent = _bonusService.GetBonusPercentage(achievement);

                var bonusAmount = worker.Salary * bonusPercent;

                result.Add(new
                {
                    worker.Id,
                    worker.Name,
                    worker.Role,
                    worker.Salary,

                    Achievement = Math.Round(achievement, 2),
                    BonusPercent = bonusPercent * 100,
                    BonusAmount = Math.Round(bonusAmount, 2),
                    TotalSalary = worker.Salary + bonusAmount
                });
            }

            return Ok(new
            {
                TotalCount = totalCount,
                SkipCount,
                MaxResultCount,
                Workers = result
            });
        }
    }
}