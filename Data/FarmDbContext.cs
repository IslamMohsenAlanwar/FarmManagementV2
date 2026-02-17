using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Models;

namespace FarmManagement.API.Data
{
    public class FarmDbContext : DbContext
    {
        public FarmDbContext(DbContextOptions<FarmDbContext> options) : base(options) { }

        // ========  Tables ========
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

        public DbSet<EggProductionRecord> EggProductionRecords { get; set; } = null!;
        public DbSet<EggSale> EggSales { get; set; } = null!;

        // ======== Workers Table ========
        public DbSet<Worker> Workers { get; set; } = null!; 

        // ======== Vacations & Advances ========
        public DbSet<Vacation> Vacations { get; set; } = null!;
        public DbSet<Advance> Advances { get; set; } = null!;

        // ================= SaveChanges Precision =================
        public override int SaveChanges() => base.SaveChanges();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            await base.SaveChangesAsync(cancellationToken);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ================= Precision Config =================
            modelBuilder.Entity<EggSale>(e =>
            {
                e.Property(s => s.Quantity).HasPrecision(18, 2);
                e.Property(s => s.UnitPrice).HasPrecision(18, 2);
                e.Property(s => s.TotalPrice).HasPrecision(18, 2);
                e.Property(s => s.PaidAmount).HasPrecision(18, 2);
                e.Property(s => s.RemainingAmount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Trader>(e => e.Property(t => t.Balance).HasPrecision(18, 2));

            modelBuilder.Entity<FeedMix>(e =>
            {
                e.Property(fm => fm.TotalWeight).HasPrecision(18, 2);
                e.Property(fm => fm.TotalPrice).HasPrecision(18, 2);
                e.Property(fm => fm.Quantity).HasPrecision(18, 2);
            });

            modelBuilder.Entity<FeedMixDetail>(e =>
            {
                e.Property(fmd => fmd.Quantity).HasPrecision(18, 2);
                e.Property(fmd => fmd.Price).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Item>(e => e.Property(i => i.PricePerTon).HasPrecision(18, 2));

            modelBuilder.Entity<WarehouseItem>(e =>
            {
                e.Property(wi => wi.Quantity).HasPrecision(18, 2);
                e.Property(wi => wi.PricePerUnit).HasPrecision(18, 2);
                e.Property(wi => wi.Withdrawn).HasPrecision(18, 2);
            });

            modelBuilder.Entity<WarehouseTransaction>(e =>
            {
                e.Property(wt => wt.Quantity).HasPrecision(18, 2);
                e.Property(wt => wt.PricePerTon).HasPrecision(18, 2);
                e.Property(wt => wt.TotalPrice).HasPrecision(18, 2);
            });

            modelBuilder.Entity<DailyFeedConsumption>(e =>
            {
                e.Property(f => f.Quantity).HasPrecision(18, 2);
                e.Property(f => f.Cost).HasPrecision(18, 2);
            });

            modelBuilder.Entity<DailyMedicineConsumption>(e =>
            {
                e.Property(m => m.Quantity).HasPrecision(18, 2);
                e.Property(m => m.Cost).HasPrecision(18, 2);
            });

            modelBuilder.Entity<EggProductionRecord>(e =>
            {
                e.Property(r => r.ProductionRate).HasPrecision(5, 2);
            });

            // ================= Workers Configuration =================
            modelBuilder.Entity<Worker>(w =>
            {
                w.HasKey(x => x.Id);
                w.Property(x => x.Name).IsRequired().HasMaxLength(100);
                w.Property(x => x.Phone).HasMaxLength(20);
                w.Property(x => x.Role).IsRequired();
                w.Property(x => x.Salary).HasPrecision(18, 2);
                w.Property(x => x.VacationDays);
            });

            // ================= Vacation Configuration =================
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
            });

            // ================= Advance Configuration =================
            modelBuilder.Entity<Advance>(a =>
            {
                a.HasKey(x => x.Id);
                a.HasOne(x => x.Worker)
                 .WithMany()
                 .HasForeignKey(x => x.WorkerId)
                 .OnDelete(DeleteBehavior.Cascade);
                a.Property(x => x.Amount).HasPrecision(18, 2);
                a.Property(x => x.Date);
            });

            // ================= Relationships (Existing) =================
            // Barn -> Farm
            modelBuilder.Entity<Barn>()
                .HasOne(b => b.Farm)
                .WithMany(f => f.Barns)
                .HasForeignKey(b => b.FarmId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cycle -> Farm & Barn
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

            // DailyRecord -> Cycle
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

            // Warehouse -> Farm
            modelBuilder.Entity<Warehouse>()
                .HasOne(w => w.Farm)
                .WithOne(f => f.Warehouse)
                .HasForeignKey<Warehouse>(w => w.FarmId);

            // FeedMixDetail -> FeedMix & Item
            modelBuilder.Entity<FeedMixDetail>()
                .HasOne(fmd => fmd.FeedMix)
                .WithMany(fm => fm.Details)
                .HasForeignKey(fmd => fmd.FeedMixId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FeedMixDetail>()
                .HasOne(fmd => fmd.Item)
                .WithMany()
                .HasForeignKey(fmd => fmd.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // WarehouseItem -> Warehouse & Item
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

            // WarehouseTransaction -> Warehouse, Trader, Item
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

            // EggProductionRecord -> Farm, Barn, Cycle
            modelBuilder.Entity<EggProductionRecord>()
                .HasOne(r => r.Farm).WithMany().HasForeignKey(r => r.FarmId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EggProductionRecord>()
                .HasOne(r => r.Barn).WithMany(b => b.EggProductionRecords).HasForeignKey(r => r.BarnId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EggProductionRecord>()
                .HasOne(r => r.Cycle).WithMany(c => c.EggProductionRecords).HasForeignKey(r => r.CycleId).OnDelete(DeleteBehavior.Cascade);

            // AssetWarehouse -> Farm
            modelBuilder.Entity<AssetWarehouse>()
                .HasOne(w => w.Farm)
                .WithOne(f => f.AssetWarehouse)
                .HasForeignKey<AssetWarehouse>(w => w.FarmId)
                .OnDelete(DeleteBehavior.Cascade);

            // AssetWarehouseItem -> AssetWarehouse & AssetItem
            modelBuilder.Entity<AssetWarehouseItem>(e =>
            {
                e.Property(a => a.Quantity).HasPrecision(18, 2);
                e.Property(a => a.InBarnsQuantity).HasPrecision(18, 2);
                e.Property(a => a.UnitPrice).HasPrecision(18, 2);
                e.Property(a => a.TotalValue).HasPrecision(18, 2);

                e.HasOne(a => a.AssetItem)
                 .WithMany()
                 .HasForeignKey(a => a.AssetItemId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(a => a.AssetWarehouse)
                 .WithMany(w => w.Items)
                 .HasForeignKey(a => a.AssetWarehouseId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // AssetTransaction -> AssetWarehouseItem & TargetBarn
            modelBuilder.Entity<AssetTransaction>(e =>
            {
                e.Property(t => t.Quantity).HasPrecision(18, 2);

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
