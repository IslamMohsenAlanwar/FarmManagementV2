using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;
using FarmManagement.API.Helpers;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CyclesController : ControllerBase
    {
        private readonly FarmDbContext _context;
        private readonly EvaluationService _evaluationService;

        public CyclesController(FarmDbContext context, EvaluationService evaluationService)
        {
            _context = context;
            _evaluationService = evaluationService;
        }

        // ================= GET ALL =================
        [HttpGet]
        public async Task<ActionResult> GetCycles(int SkipCount = 0, int MaxResultCount = 7) // القيمة الافتراضية 7
        {
            var query = _context.Cycles
                .Include(c => c.Farm)
                .Include(c => c.Barn)
                .Include(c => c.BarnManager)
                .Include(c => c.BarnWorker)
                .Include(c => c.Breed)
                .Include(c => c.Evaluations)
                    .ThenInclude(e => e.Details)
                .OrderByDescending(c => c.Id);

            var totalCount = await query.CountAsync();

            var cyclesList = await query
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .ToListAsync();

            var result = cyclesList.Select(c => new CycleDto
            {
                Id = c.Id,
                Name = c.Name,
                FarmId = c.FarmId,
                FarmName = c.Farm.Name,
                BarnId = c.BarnId,
                BarnName = c.Barn.Name,
                BreedId = c.BreedId,
                BreedName = c.Breed != null ? c.Breed.Name : "",

                BarnManagerId = c.BarnManagerId,
                BarnManagerName = c.BarnManager?.Name,


                BarnWorkerId = c.BarnWorkerId,
                BarnWorkerName = c.BarnWorker?.Name,

                StartDate = c.StartDate,
                EndDate = c.EndDate,
                ChickCount = c.ChickCount,
                ChickAge = c.ChickAge,

                FinalScore = _evaluationService.CalculateFinalScore(c)
            }).ToList();

            return Ok(new
            {
                TotalCount = totalCount,
                Cycles = result
            });
        }

        // ================= GET BY ID =================
        [HttpGet("{id}")]
        public async Task<ActionResult<CycleDto>> GetCycle(int id)
        {
            var c = await _context.Cycles
                .Include(c => c.Farm)
                .Include(c => c.Barn)
                .Include(c => c.BarnManager)
                .Include(c => c.BarnWorker)
                .Include(c => c.Breed)
                .Include(c => c.Evaluations)
                .ThenInclude(e => e.Details)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (c == null) return NotFound();

            var dto = new CycleDto
            {
                Id = c.Id,
                Name = c.Name,
                FarmId = c.FarmId,
                FarmName = c.Farm.Name,
                BarnId = c.BarnId,
                BarnName = c.Barn.Name,
                BreedId = c.BreedId,
                BreedName = c.Breed != null ? c.Breed.Name : "",

                BarnManagerId = c.BarnManagerId,
                BarnManagerName = c.BarnManager?.Name,

                BarnWorkerId = c.BarnWorkerId,
                BarnWorkerName = c.BarnWorker?.Name,

                StartDate = c.StartDate,
                EndDate = c.EndDate,
                ChickCount = c.ChickCount,
                ChickAge = c.ChickAge,

                FinalScore = _evaluationService.CalculateFinalScore(c)
            };

            return Ok(dto);
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<ActionResult<CycleDto>> CreateCycle(CycleCreateDto dto)
        {
            // التحقق من وجود Farm و Barn
            if (!await _context.Farms.AnyAsync(f => f.Id == dto.FarmId))
                return BadRequest("Farm not found");
            if (!await _context.Barns.AnyAsync(b => b.Id == dto.BarnId))
                return BadRequest("Barn not found");

            // تحقق من صلاحية BarnManager
            if (dto.BarnManagerId.HasValue &&
                !await _context.Workers.AnyAsync(u => u.Id == dto.BarnManagerId && u.Role == WorkerRole.BarnManager))
                return BadRequest("Invalid Barn Manager");

            // تحقق من صلاحية BarnWorker
            if (dto.BarnWorkerId.HasValue &&
                !await _context.Workers.AnyAsync(u => u.Id == dto.BarnWorkerId && u.Role == WorkerRole.BarnWorker))
                return BadRequest("Invalid Barn Worker");

            //  تحقق من وجود Breed
            if (!await _context.Breeds.AnyAsync(b => b.Id == dto.BreedId))
                return BadRequest("Breed not found");

            // إنشاء الدورة
            var cycle = new Cycle
            {
                Name = dto.Name,
                FarmId = dto.FarmId,
                BarnId = dto.BarnId,
                BarnManagerId = dto.BarnManagerId,
                BarnWorkerId = dto.BarnWorkerId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ChickCount = dto.ChickCount,
                ChickAge = dto.ChickAge,
                BreedId = dto.BreedId // ربط السلالة هنا
            };

            _context.Cycles.Add(cycle);
            await _context.SaveChangesAsync();

            // إرجاع الـ DTO
            var createdCycle = await _context.Cycles
                .Include(c => c.Farm)
                .Include(c => c.Barn)
                .Include(c => c.BarnManager)
                .Include(c => c.BarnWorker)
                .Include(c => c.Breed) // جلب اسم السلالة
                .Include(c => c.Evaluations)
                    .ThenInclude(e => e.Details)
                .FirstOrDefaultAsync(c => c.Id == cycle.Id);

            var dtoResult = new CycleDto
            {
                Id = createdCycle.Id,
                Name = createdCycle.Name,
                FarmId = createdCycle.FarmId,
                FarmName = createdCycle.Farm.Name,
                BarnId = createdCycle.BarnId,
                BarnName = createdCycle.Barn.Name,
                BarnManagerId = createdCycle.BarnManagerId,
                BarnManagerName = createdCycle.BarnManager?.Name,
                BarnWorkerId = createdCycle.BarnWorkerId,
                BarnWorkerName = createdCycle.BarnWorker?.Name,
                StartDate = createdCycle.StartDate,
                EndDate = createdCycle.EndDate,
                ChickCount = createdCycle.ChickCount,
                ChickAge = createdCycle.ChickAge,
                BreedId = createdCycle.BreedId,            // إضافة BreedId
                BreedName = createdCycle.Breed.Name,       // إضافة اسم السلالة
                FinalScore = _evaluationService.CalculateFinalScore(createdCycle)
            };

            return CreatedAtAction(nameof(GetCycle), new { id = dtoResult.Id }, dtoResult);
        }

        // ================= UPDATE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCycle(int id, CycleUpdateDto dto)
        {
            var cycle = await _context.Cycles.FindAsync(id);
            if (cycle == null) return NotFound();

            cycle.Name = dto.Name;
            cycle.FarmId = dto.FarmId;
            cycle.BarnId = dto.BarnId;
            cycle.StartDate = dto.StartDate;
            cycle.EndDate = dto.EndDate;
            cycle.ChickCount = dto.ChickCount;
            cycle.ChickAge = dto.ChickAge;
            cycle.BarnManagerId = dto.BarnManagerId;
            cycle.BarnWorkerId = dto.BarnWorkerId;
            cycle.BreedId = dto.BreedId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ================= DELETE =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCycle(int id)
        {
            var cycle = await _context.Cycles.FindAsync(id);
            if (cycle == null)
                return NotFound();

            // =========================
            // التحقق من وجود سجلات يومية
            // =========================
            var hasDailyRecords = await _context.DailyRecords
                .AnyAsync(r => r.CycleId == id);

            if (hasDailyRecords)
                return BadRequest("لا يمكن حذف الدورة لأنها تحتوي على سجلات يومية.");

            _context.Cycles.Remove(cycle);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}