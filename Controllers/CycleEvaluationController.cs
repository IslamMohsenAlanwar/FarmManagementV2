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
    public class CycleEvaluationController : ControllerBase
    {
        private readonly FarmDbContext _context;
        private readonly EvaluationService _evaluationService;

        public CycleEvaluationController(FarmDbContext context, EvaluationService evaluationService)
        {
            _context = context;
            _evaluationService = evaluationService;
        }

        // ================= إضافة تقييم يومي لدورة =================
        [HttpPost]
        public async Task<IActionResult> AddEvaluation([FromBody] CycleEvaluationCreateDto dto)
        {
            var cycle = await _context.Cycles
                .Include(c => c.Evaluations)
                .FirstOrDefaultAsync(c => c.Id == dto.CycleId);

            if (cycle == null)
                return NotFound("Cycle not found");

            var evaluation = new CycleEvaluation
            {
                CycleId = dto.CycleId,
                Date = DateTime.Now
            };

            foreach (var score in dto.Scores)
            {
                evaluation.Details.Add(new CycleEvaluationDetail
                {
                    EvaluationItemId = score.EvaluationItemId,
                    Score = score.Score
                });
            }

            _context.CycleEvaluations.Add(evaluation);
            await _context.SaveChangesAsync();

            return Ok(evaluation);
        }

        // ================= جلب جميع تقييمات الدورة =================
        [HttpGet("{cycleId}")]
        public async Task<ActionResult<List<CycleEvaluation>>> GetEvaluations(int cycleId)
        {
            var evaluations = await _context.CycleEvaluations
                .Where(e => e.CycleId == cycleId)
                .Include(e => e.Details)
                .ThenInclude(d => d.EvaluationItem)
                .ToListAsync();

            return Ok(evaluations);
        }

        // ================= حساب التقييم النهائي للدورة =================
        [HttpGet("{cycleId}/final-score")]
        public async Task<ActionResult<CycleFinalScoreDto>> GetFinalScore(int cycleId)
        {
            var cycle = await _context.Cycles
                .Include(c => c.Evaluations)
                .ThenInclude(e => e.Details)
                .FirstOrDefaultAsync(c => c.Id == cycleId);

            if (cycle == null)
                return NotFound("Cycle not found");

            double finalScore = _evaluationService.CalculateFinalScore(cycle);

            return Ok(new CycleFinalScoreDto
            {
                CycleId = cycleId,
                FinalScore = finalScore
            });
        }
    }
}