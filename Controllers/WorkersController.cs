using Microsoft.AspNetCore.Mvc;
using FarmManagement.API.Helpers;
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
        private readonly EvaluationService _evaluationService;

        // ===== Constructor مع DI لـ EvaluationService =====
        public WorkersController(FarmDbContext context, EvaluationService evaluationService)
        {
            _context = context;
            _evaluationService = evaluationService;
        }

        private static WorkerRole RoleFromNumber(int number) => number switch
        {
            1 => WorkerRole.FarmManager,
            2 => WorkerRole.BarnManager,
            3 => WorkerRole.BarnWorker,
            _ => throw new ArgumentException("Invalid role number")
        };

        private static string RoleToString(WorkerRole role) => role switch
        {
            WorkerRole.FarmManager => "FarmManager",
            WorkerRole.BarnManager => "BarnManager",
            WorkerRole.BarnWorker => "BarnWorker",
            _ => "Unknown"
        };

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
                VacationDays = worker.VacationDays,
                FinalScore = 0
            };

            return CreatedAtAction(nameof(GetWorkerById), new { id = worker.Id }, result);
        }

        // ===== GET ALL WITH FinalScore =====
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkerDto>>> GetWorkers()
        {
            var workers = await _context.Workers
                .OrderByDescending(w => w.Id)
                .ToListAsync();

            var result = workers.Select(w =>
            {
                double finalScore = 0;

                // جلب آخر دورة مرتبطة بالـ Worker
                var lastCycle = _context.Cycles
                    .Include(c => c.Evaluations)
                    .ThenInclude(e => e.Details)
                    .Where(c => (w.Role == WorkerRole.BarnWorker && c.BarnWorkerId == w.Id) ||
                                (w.Role == WorkerRole.BarnManager && c.BarnManagerId == w.Id))
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefault();

                if (lastCycle != null)
                {
                    // حساب FinalScore باستخدام EvaluationService
                    finalScore = _evaluationService.CalculateFinalScore(lastCycle);
                }

                return new WorkerDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Phone = w.Phone,
                    Role = RoleToString(w.Role),
                    Salary = w.Salary,
                    VacationDays = w.VacationDays,
                    FinalScore = finalScore
                };
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WorkerDto>> GetWorkerById(int id)
        {
            var worker = await _context.Workers.FindAsync(id);
            if (worker == null) return NotFound();

            // جلب آخر دورة مرتبطة
            double finalScore = 0;
            var lastCycle = _context.Cycles
                .Include(c => c.Evaluations)
                .ThenInclude(e => e.Details)
                .Where(c => (worker.Role == WorkerRole.BarnWorker && c.BarnWorkerId == worker.Id) ||
                            (worker.Role == WorkerRole.BarnManager && c.BarnManagerId == worker.Id))
                .OrderByDescending(c => c.Id)
                .FirstOrDefault();

            if (lastCycle != null)
                finalScore = _evaluationService.CalculateFinalScore(lastCycle);

            var result = new WorkerDto
            {
                Id = worker.Id,
                Name = worker.Name,
                Phone = worker.Phone,
                Role = RoleToString(worker.Role),
                Salary = worker.Salary,
                VacationDays = worker.VacationDays,
                FinalScore = finalScore
            };

            return Ok(result);
        }

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

        [HttpGet("roles")]
        public ActionResult<IEnumerable<WorkerRoleLookupDto>> GetWorkerRoles()
        {
            var roles = Enum.GetValues(typeof(WorkerRole))
                .Cast<WorkerRole>()
                .Select(r => new WorkerRoleLookupDto
                {
                    Id = (int)r,
                    Name = r.ToString()
                })
                .ToList();

            return Ok(roles);
        }

        [HttpGet("barn-managers")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetBarnManagers()
        {
            var managers = await _context.Workers
                .Where(w => w.Role == WorkerRole.BarnManager)
                .Select(w => new LookupDto
                {
                    Id = w.Id,
                    Name = w.Name
                })
                .ToListAsync();

            return Ok(managers);
        }

        [HttpGet("barn-workers")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetBarnWorkers()
        {
            var workers = await _context.Workers
                .Where(w => w.Role == WorkerRole.BarnWorker)
                .Select(w => new LookupDto
                {
                    Id = w.Id,
                    Name = w.Name
                })
                .ToListAsync();

            return Ok(workers);
        }

        // ===================== VACATIONS =====================
        [HttpPost("vacation")]
        public async Task<IActionResult> AddVacation([FromBody] CreateVacationDto dto)
        {
            var worker = await _context.Workers.FindAsync(dto.WorkerId);
            if (worker == null) return NotFound("Worker not found");

            int daysRequested = (dto.EndDate - dto.StartDate).Days + 1;

            if (daysRequested > worker.VacationDays)
                return BadRequest("عدد أيام الإجازة أكبر من المتاح");

            worker.VacationDays -= daysRequested;

            var lastCumulative = await _context.Vacations
                .Where(v => v.WorkerId == dto.WorkerId)
                .OrderByDescending(v => v.Id)
                .Select(v => v.CumulativeDays)
                .FirstOrDefaultAsync();

            var vacation = new Vacation
            {
                WorkerId = worker.Id,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Days = daysRequested,
                CumulativeDays = lastCumulative + daysRequested
            };

            _context.Vacations.Add(vacation);
            await _context.SaveChangesAsync();

            return Ok(vacation);
        }

        [HttpGet("vacations")]
        public async Task<ActionResult<IEnumerable<VacationRecordDto>>> GetAllVacations()
        {
            var vacations = await _context.Vacations
                .Include(v => v.Worker)
                .OrderBy(v => v.StartDate)
                .Select(v => new VacationRecordDto
                {
                    Id = v.Id,
                    WorkerName = v.Worker!.Name,
                    StartDate = v.StartDate,
                    EndDate = v.EndDate,
                    Days = v.Days,
                    CumulativeDays = v.CumulativeDays
                })
                .ToListAsync();

            return Ok(vacations);
        }

        [HttpGet("vacations/{workerId}")]
        public async Task<ActionResult<IEnumerable<VacationRecordDto>>> GetVacationsByWorker(int workerId)
        {
            var worker = await _context.Workers.FindAsync(workerId);
            if (worker == null) return NotFound("Worker not found");

            var vacations = await _context.Vacations
                .Where(v => v.WorkerId == workerId)
                .OrderBy(v => v.StartDate)
                .Select(v => new VacationRecordDto
                {
                    Id = v.Id,
                    WorkerName = worker.Name,
                    StartDate = v.StartDate,
                    EndDate = v.EndDate,
                    Days = v.Days,
                    CumulativeDays = v.CumulativeDays
                })
                .ToListAsync();

            return Ok(vacations);
        }

        // ===================== ADVANCES =====================
       [HttpPost("advance")]
public async Task<IActionResult> AddAdvance([FromBody] CreateAdvanceDto dto)
{
    await using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        var worker = await _context.Workers.FindAsync(dto.WorkerId);
        if (worker == null) 
            return NotFound("Worker not found");

        var lastCumulative = await _context.Advances
            .Where(a => a.WorkerId == dto.WorkerId)
            .OrderByDescending(a => a.Id)
            .Select(a => a.CumulativeAmount)
            .FirstOrDefaultAsync();

        var advance = new Advance
        {
            WorkerId = worker.Id,
            Amount = dto.Amount,
            Date = dto.Date,
            CumulativeAmount = lastCumulative + dto.Amount
        };

        _context.Advances.Add(advance);

        // ✅ تسجيلها كمصروف في الخزنة
        var cashBoxEntry = new CashBoxTransaction
        {
            Date = dto.Date,
            Type = "منصرف",
            Category = "سلفة",
            Amount = dto.Amount,
            Notes = $"سلفة للعامل {worker.Name}",
            WorkerId = worker.Id
        };

        _context.CashBoxTransactions.Add(cashBoxEntry);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            message = "تم تسجيل السلفة وخصمها من الخزنة",
            advance
        });
    }
    catch
    {
        await transaction.RollbackAsync();
        return BadRequest("حدث خطأ أثناء تسجيل السلفة");
    }
}

        [HttpGet("advances")]
        public async Task<ActionResult<IEnumerable<AdvanceRecordDto>>> GetAllAdvances()
        {
            var advances = await _context.Advances
                .Include(a => a.Worker)
                .OrderBy(a => a.Date)
                .Select(a => new AdvanceRecordDto
                {
                    Id = a.Id,
                    WorkerName = a.Worker!.Name,
                    Amount = a.Amount,
                    Date = a.Date,
                    CumulativeAmount = a.CumulativeAmount
                })
                .ToListAsync();

            return Ok(advances);
        }

        [HttpGet("advances/{workerId}")]
        public async Task<ActionResult<IEnumerable<AdvanceRecordDto>>> GetAdvancesByWorker(int workerId)
        {
            var worker = await _context.Workers.FindAsync(workerId);
            if (worker == null) return NotFound("Worker not found");

            var advances = await _context.Advances
                .Where(a => a.WorkerId == workerId)
                .OrderBy(a => a.Date)
                .Select(a => new AdvanceRecordDto
                {
                    Id = a.Id,
                    WorkerName = worker.Name,
                    Amount = a.Amount,
                    Date = a.Date,
                    CumulativeAmount = a.CumulativeAmount
                })
                .ToListAsync();

            return Ok(advances);
        }

[HttpPost("salary")]
public async Task<IActionResult> PayMonthlySalary([FromBody] CreateSalaryDto dto)
{
    await using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        var worker = await _context.Workers.FindAsync(dto.WorkerId);
        if (worker == null)
            return NotFound("Worker not found");

        // منع تكرار صرف نفس الشهر
        var alreadyPaid = await _context.Salaries.AnyAsync(s =>
            s.WorkerId == dto.WorkerId &&
            s.Month == dto.Month &&
            s.Year == dto.Year);

        if (alreadyPaid)
            return BadRequest("تم صرف راتب هذا الشهر بالفعل");

        // إجمالي السلف الحالية
        var totalAdvances = await _context.Advances
            .Where(a => a.WorkerId == dto.WorkerId)
            .OrderByDescending(a => a.Id)
            .Select(a => a.CumulativeAmount)
            .FirstOrDefaultAsync();

        var baseSalary = worker.Salary;
        var netSalary = baseSalary - totalAdvances;
        if (netSalary < 0) netSalary = 0;

        var salary = new Salary
        {
            WorkerId = worker.Id,
            Month = dto.Month,
            Year = dto.Year,
            BaseSalary = baseSalary,
            TotalAdvances = totalAdvances,
            NetSalary = netSalary,
            Date = DateTime.Now
        };

        _context.Salaries.Add(salary);

        // تسجيل مصروف في الخزنة
        var cashBoxEntry = new CashBoxTransaction
        {
            Date = DateTime.Now,
            Type = "منصرف",
            Category = "راتب",
            Amount = netSalary,
            Notes = $"راتب شهر {dto.Month}/{dto.Year} للعامل {worker.Name}",
            WorkerId = worker.Id
        };

        _context.CashBoxTransactions.Add(cashBoxEntry);

        // تصفير السلف بعد الخصم
        if (totalAdvances > 0)
        {
            _context.Advances.Add(new Advance
            {
                WorkerId = worker.Id,
                Amount = 0,
                Date = DateTime.Now,
                CumulativeAmount = 0
            });
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            message = "تم صرف الراتب بنجاح",
            baseSalary,
            totalAdvances,
            netSalary
        });
    }
    catch
    {
        await transaction.RollbackAsync();
        return BadRequest("حدث خطأ أثناء صرف الراتب");
    }
}

    }

}



