using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Models;

namespace FarmManagement.API.Data
{
    public class FarmDbContext : DbContext
    {
        public FarmDbContext(DbContextOptions<FarmDbContext> options) : base(options) { }

        // ======== Tables ========
        public DbSet<Farm> Farms { get; set; } = null!;
        public DbSet<Barn> Barns { get; set; } = null!;
        public DbSet<Cycle> Cycles { get; set; } = null!;
        public DbSet<DailyRecord> DailyRecords { get; set; } = null!;
        public DbSet<DailyFeedConsumption> DailyFeedConsumptions { get; set; } = null!;
        public DbSet<DailyMedicineConsumption> DailyMedicineConsumptions { get; set; } = null!;
        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<FeedMix> FeedMixes { get; set; } = null!;
        public DbSet<FeedMixDetail> FeedMixDetails { get; set; } = null!;
        public DbSet<FeedType> FeedTypes { get; set; } = null!;
        public DbSet<Trader> Traders { get; set; } = null!;
        public DbSet<Warehouse> Warehouses { get; set; } = null!;
        public DbSet<WarehouseItem> WarehouseItems { get; set; } = null!;
        public DbSet<WarehouseTransaction> WarehouseTransactions { get; set; } = null!;

        // ======== Asset Tables ========
        public DbSet<AssetItem> AssetItems { get; set; } = null!;
        public DbSet<AssetWarehouse> AssetWarehouses { get; set; } = null!;
        public DbSet<AssetWarehouseItem> AssetWarehouseItems { get; set; } = null!;
        public DbSet<AssetTransaction> AssetTransactions { get; set; } = null!;

        // ======== Eggs ========
        public DbSet<EggProductionRecord> EggProductionRecords { get; set; } = null!;
        public DbSet<EggSale> EggSales { get; set; } = null!;

        // ======== Workers ========
        public DbSet<Worker> Workers { get; set; } = null!;
        public DbSet<Vacation> Vacations { get; set; } = null!;
        public DbSet<Advance> Advances { get; set; } = null!;
        public DbSet<ChickenSale> ChickenSales { get; set; } = null!;

 // ======== Evaluation ========
public DbSet<EvaluationItem> EvaluationItems { get; set; } = null!;
public DbSet<CycleEvaluation> CycleEvaluations { get; set; } = null!;
public DbSet<CycleEvaluationDetail> CycleEvaluationDetails { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =====================================================
            // GLOBAL DECIMAL PRECISION
            // =====================================================
            foreach (var property in modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }

            // ================= Workers =================
            modelBuilder.Entity<Worker>(w =>
            {
                w.HasKey(x => x.Id);
                w.Property(x => x.Name).IsRequired().HasMaxLength(100);
                w.Property(x => x.Phone).HasMaxLength(20);
                w.Property(x => x.Role).IsRequired();
                w.Property(x => x.Salary);
                w.Property(x => x.VacationDays);
            });

            // ================= Vacation =================
            modelBuilder.Entity<Vacation>(v =>
            {
                v.HasKey(x => x.Id);
                v.HasOne(x => x.Worker)
                 .WithMany()
                 .HasForeignKey(x => x.WorkerId)
                 .OnDelete(DeleteBehavior.Cascade);
                v.Property(x => x.Days);
                v.Property(x => x.StartDate);
                v.Property(x => x.EndDate);
                v.Property(x => x.CumulativeDays);
            });

            // ================= Advance =================
            modelBuilder.Entity<Advance>(a =>
            {
                a.HasKey(x => x.Id);
                a.HasOne(x => x.Worker)
                 .WithMany()
                 .HasForeignKey(x => x.WorkerId)
                 .OnDelete(DeleteBehavior.Cascade);
                a.Property(x => x.Amount);
                a.Property(x => x.CumulativeAmount);
                a.Property(x => x.Date);
            });

            // ================= Barn =================
            modelBuilder.Entity<Barn>()
                .HasOne(b => b.Farm)
                .WithMany(f => f.Barns)
                .HasForeignKey(b => b.FarmId)
                .OnDelete(DeleteBehavior.Cascade);

            // ================= Cycle =================
            modelBuilder.Entity<Cycle>()
                .HasOne(c => c.Farm)
                .WithMany(f => f.Cycles)
                .HasForeignKey(c => c.FarmId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cycle>()
                .HasOne(c => c.Barn)
                .WithMany(b => b.Cycles)
                .HasForeignKey(c => c.BarnId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cycle>()
                .HasOne(c => c.BarnManager)
                .WithMany()
                .HasForeignKey(c => c.BarnManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cycle>()
                .HasOne(c => c.BarnWorker)
                .WithMany()
                .HasForeignKey(c => c.BarnWorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================= DailyRecord =================
            modelBuilder.Entity<DailyRecord>()
                .HasOne(d => d.Cycle)
                .WithMany(c => c.DailyRecords)
                .HasForeignKey(d => d.CycleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DailyRecord>()
                .HasMany(d => d.FeedConsumptions)
                .WithOne(f => f.DailyRecord)
                .HasForeignKey(f => f.DailyRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DailyRecord>()
                .HasMany(d => d.MedicineConsumptions)
                .WithOne(m => m.DailyRecord)
                .HasForeignKey(m => m.DailyRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            // ================= Warehouse =================
            modelBuilder.Entity<Warehouse>()
                .HasOne(w => w.Farm)
                .WithOne(f => f.Warehouse)
                .HasForeignKey<Warehouse>(w => w.FarmId);

            modelBuilder.Entity<WarehouseItem>()
                .HasOne(wi => wi.Warehouse)
                .WithMany(w => w.WarehouseItems)
                .HasForeignKey(wi => wi.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WarehouseItem>()
                .HasOne(wi => wi.Item)
                .WithMany(i => i.WarehouseItems)
                .HasForeignKey(wi => wi.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WarehouseTransaction>()
                .HasOne(wt => wt.Warehouse)
                .WithMany(w => w.Transactions)
                .HasForeignKey(wt => wt.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WarehouseTransaction>()
                .HasOne(wt => wt.Trader)
                .WithMany(t => t.Transactions)
                .HasForeignKey(wt => wt.TraderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WarehouseTransaction>()
                .HasOne(wt => wt.Item)
                .WithMany()
                .HasForeignKey(wt => wt.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================= EggProductionRecord =================
            modelBuilder.Entity<EggProductionRecord>()
                .HasOne(r => r.Farm)
                .WithMany() // لا حاجة لجمع EggProductionRecords في Farm
                .HasForeignKey(r => r.FarmId)
                .OnDelete(DeleteBehavior.Restrict); // منع Multiple Cascade Paths

            modelBuilder.Entity<EggProductionRecord>()
                .HasOne(r => r.Barn)
                .WithMany(b => b.EggProductionRecords)
                .HasForeignKey(r => r.BarnId)
                .OnDelete(DeleteBehavior.Restrict); // منع Multiple Cascade Paths

            modelBuilder.Entity<EggProductionRecord>()
                .HasOne(r => r.Cycle)
                .WithMany(c => c.EggProductionRecords)
                .HasForeignKey(r => r.CycleId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade فقط من Cycle


// ================= EvaluationItem =================
modelBuilder.Entity<EvaluationItem>(e =>
{
    e.HasKey(x => x.Id);
    e.Property(x => x.Name).IsRequired().HasMaxLength(100);
});

modelBuilder.Entity<CycleEvaluation>(e =>
{
    e.HasKey(x => x.Id);
    e.HasOne(x => x.Cycle)
     .WithMany(c => c.Evaluations)
     .HasForeignKey(x => x.CycleId)
     .OnDelete(DeleteBehavior.Cascade);
});

modelBuilder.Entity<CycleEvaluationDetail>(e =>
{
    e.HasKey(x => x.Id);
    e.HasOne(d => d.CycleEvaluation)
     .WithMany(c => c.Details)
     .HasForeignKey(d => d.CycleEvaluationId)
     .OnDelete(DeleteBehavior.Cascade);

    e.HasOne(d => d.EvaluationItem)
     .WithMany()
     .HasForeignKey(d => d.EvaluationItemId)
     .OnDelete(DeleteBehavior.Restrict);
});

            // ================= Assets =================
            modelBuilder.Entity<AssetWarehouse>()
                .HasOne(w => w.Farm)
                .WithOne(f => f.AssetWarehouse)
                .HasForeignKey<AssetWarehouse>(w => w.FarmId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssetWarehouseItem>(e =>
            {
                e.Property(a => a.Quantity);
                e.Property(a => a.InBarnsQuantity);
                e.Property(a => a.UnitPrice);
                e.Property(a => a.TotalValue);

                e.HasOne(a => a.AssetItem)
                 .WithMany()
                 .HasForeignKey(a => a.AssetItemId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(a => a.AssetWarehouse)
                 .WithMany(w => w.Items)
                 .HasForeignKey(a => a.AssetWarehouseId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AssetTransaction>(e =>
            {
                e.Property(t => t.Quantity);

                e.HasOne(t => t.AssetWarehouseItem)
                 .WithMany()
                 .HasForeignKey(t => t.AssetWarehouseItemId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(t => t.TargetBarn)
                 .WithMany()
                 .HasForeignKey(t => t.TargetBarnId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}