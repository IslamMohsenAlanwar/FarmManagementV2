using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
namespace FarmManagement.API.Services


{
    public class BonusService
    {
        private readonly FarmDbContext _context;

        public BonusService(FarmDbContext context)
        {
            _context = context;
        }

        // ================= BONUS RULE =================
        public decimal GetBonusPercentage(decimal achievement)
        {
            if (achievement >= 99) return 0.40m;
            if (achievement >= 98) return 0.20m;
            return 0m;
        }

        // ================= MAIN LOGIC =================
        public async Task<decimal> GetWorkerAchievement(int workerId, WorkerRole role)
        {
            // ===== Barn Worker / Barn Manager =====
            if (role == WorkerRole.BarnWorker || role == WorkerRole.BarnManager)
            {
                var cycle = await _context.Cycles
                    .Where(c =>
                        (role == WorkerRole.BarnWorker && c.BarnWorkerId == workerId) ||
                        (role == WorkerRole.BarnManager && c.BarnManagerId == workerId))
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefaultAsync();

                if (cycle == null) return 0;

                return await GetCycleAchievement(cycle.Id);
            }

            // ===== Farm Manager =====
            if (role == WorkerRole.FarmManager)
            {
                var cycles = await _context.Cycles
                    .Where(c => c.EndDate == null || c.EndDate >= DateTime.Now)
                    .ToListAsync();

                if (!cycles.Any()) return 0;

                var list = new List<decimal>();

                foreach (var c in cycles)
                {
                    var val = await GetCycleAchievement(c.Id);
                    list.Add(val);
                }

                return list.Average();
            }

            return 0;
        }

        // ================= CYCLE ACHIEVEMENT =================
        private async Task<decimal> GetCycleAchievement(int cycleId)
        {
            var cycle = await _context.Cycles
                .Include(c => c.DailyRecords)
                .Include(c => c.EggProductionRecords)
                    .ThenInclude(e => e.Details)
                .FirstOrDefaultAsync(c => c.Id == cycleId);

            if (cycle == null) return 0;

            decimal cumulativeActual = 0;
            decimal cumulativeTarget = 0;

            var settings = await _context.EggProductionSettings
                .Where(x => x.BreedId == cycle.BreedId)
                .ToListAsync();

            foreach (var day in cycle.DailyRecords)
            {
                var week = (int)Math.Ceiling(day.ChickAge / 7.0);

                var setting = settings.FirstOrDefault(s =>
                    s.WeekStart <= week && s.WeekEnd >= week);

                var targetPercent = setting?.TargetProductionPercent ?? 0;

                var targetEggs = day.RemainingChicks * (targetPercent / 100m);
                var targetCartons = targetEggs / 30m;

                var eggRecord = cycle.EggProductionRecords
                    .FirstOrDefault(e => e.Date.Date == day.Date.Date);

                decimal actualCartons = 0;

                if (eggRecord != null)
                    actualCartons = eggRecord.Details.Sum(d => d.CartonsCount);

                cumulativeActual += actualCartons;
                cumulativeTarget += targetCartons;
            }

            if (cumulativeTarget == 0) return 0;

            return (cumulativeActual / cumulativeTarget) * 100;
        }
    }
}
