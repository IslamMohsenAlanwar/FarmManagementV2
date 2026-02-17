using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;

namespace FarmManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkersController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public WorkersController(FarmDbContext context)
        {
            _context = context;
        }

        // ======= تحويل الرقم للـ enum =======
        private static WorkerRole RoleFromNumber(int number) => number switch
        {
            1 => WorkerRole.FarmManager,
            2 => WorkerRole.BarnManager,
            3 => WorkerRole.BarnWorker,
            _ => throw new ArgumentException("Invalid role number")
        };

        // ======= تحويل الـ enum لنص =======
        private static string RoleToString(WorkerRole role) => role switch
        {
            WorkerRole.FarmManager => "FarmManager",
            WorkerRole.BarnManager => "BarnManager",
            WorkerRole.BarnWorker => "BarnWorker",
            _ => "Unknown"
        };

        // ======= إنشاء عامل جديد =======
        [HttpPost]
        public async Task<ActionResult<WorkerDto>> AddWorker([FromBody] CreateWorkerDto dto)
        {
            var worker = new Worker
            {
                Name = dto.Name,
                Phone = dto.Phone,
                Role = RoleFromNumber(dto.Role),
                Salary = dto.Salary,
                VacationDays = dto.VacationDays
            };

            _context.Workers.Add(worker);
            await _context.SaveChangesAsync();

            var result = new WorkerDto
            {
                Id = worker.Id,
                Name = worker.Name,
                Phone = worker.Phone,
                Role = RoleToString(worker.Role),
                Salary = worker.Salary,
                VacationDays = worker.VacationDays
            };

            return CreatedAtAction(nameof(GetWorkerById), new { id = worker.Id }, result);
        }

        // ======= جلب كل العمال =======
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkerDto>>> GetWorkers()
        {
            var workers = await _context.Workers.ToListAsync();

            var result = workers.Select(w => new WorkerDto
            {
                Id = w.Id,
                Name = w.Name,
                Phone = w.Phone,
                Role = RoleToString(w.Role),
                Salary = w.Salary,
                VacationDays = w.VacationDays
            });

            return Ok(result);
        }

        // ======= جلب عامل بالـ id =======
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkerDto>> GetWorkerById(int id)
        {
            var worker = await _context.Workers.FindAsync(id);
            if (worker == null) return NotFound();

            var result = new WorkerDto
            {
                Id = worker.Id,
                Name = worker.Name,
                Phone = worker.Phone,
                Role = RoleToString(worker.Role),
                Salary = worker.Salary,
                VacationDays = worker.VacationDays
            };

            return Ok(result);
        }

        // ======= تحديث عامل =======
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorker(int id, [FromBody] UpdateWorkerDto dto)
        {
            var worker = await _context.Workers.FindAsync(id);
            if (worker == null) return NotFound();

            worker.Name = dto.Name;
            worker.Phone = dto.Phone;
            worker.Role = RoleFromNumber(dto.Role);
            worker.Salary = dto.Salary;
            worker.VacationDays = dto.VacationDays;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ======= إضافة إجازة =======
        [HttpPost("vacation")]
        public async Task<IActionResult> AddVacation([FromBody] CreateVacationDto dto)
        {
            var worker = await _context.Workers.FindAsync(dto.WorkerId);
            if (worker == null) return NotFound("Worker not found");

            int daysRequested = (dto.EndDate - dto.StartDate).Days + 1;
            if (daysRequested > worker.VacationDays)
                return BadRequest("عدد أيام الإجازة أكبر من المتاح");

            worker.VacationDays -= daysRequested;

            var vacation = new Vacation
            {
                WorkerId = worker.Id,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Days = daysRequested,
                Worker = null // nullable، مش محتاج تحددها
            };

            _context.Vacations.Add(vacation);
            await _context.SaveChangesAsync();
            return Ok(vacation);
        }

        // ======= إضافة سلفة =======
        [HttpPost("advance")]
        public async Task<IActionResult> AddAdvance([FromBody] CreateAdvanceDto dto)
        {
            var worker = await _context.Workers.FindAsync(dto.WorkerId);
            if (worker == null) return NotFound("Worker not found");

            var advance = new Advance
            {
                WorkerId = worker.Id,
                Amount = dto.Amount,
                Date = dto.Date,
                Worker = null // nullable، مش محتاج تحددها
            };

            _context.Advances.Add(advance);
            await _context.SaveChangesAsync();
            return Ok(advance);
        }
    }
}
