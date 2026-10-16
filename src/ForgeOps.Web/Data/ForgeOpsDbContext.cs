using ForgeOps.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ForgeOps.Web.Data;

public class ForgeOpsDbContext(DbContextOptions<ForgeOpsDbContext> options) : DbContext(options)
{
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<Technician> Technicians => Set<Technician>();
    public DbSet<WorkOrderAudit> WorkOrderAudits => Set<WorkOrderAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var orders = modelBuilder.Entity<WorkOrder>();
        orders.Property(x => x.Title).HasMaxLength(120).IsRequired();
        orders.Property(x => x.EquipmentName).HasMaxLength(120).IsRequired();
        orders.Property(x => x.Facility).HasMaxLength(100).IsRequired();
        orders.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        orders.Property(x => x.ResolutionNotes).HasMaxLength(4000);
        orders.HasIndex(x => new { x.IsArchived, x.Status, x.DueDate });
        orders.HasIndex(x => x.UpdatedAtUtc);
        orders.HasOne(x => x.AssignedTechnician).WithMany()
            .HasForeignKey(x => x.AssignedTechnicianId).OnDelete(DeleteBehavior.Restrict);
        orders.HasMany(x => x.History).WithOne(x => x.WorkOrder)
            .HasForeignKey(x => x.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
        orders.Property(x => x.CreatedAtUtc).HasConversion(x => x, x => DateTime.SpecifyKind(x, DateTimeKind.Utc));
        orders.Property(x => x.UpdatedAtUtc).HasConversion(x => x, x => DateTime.SpecifyKind(x, DateTimeKind.Utc));
        var audits = modelBuilder.Entity<WorkOrderAudit>();
        audits.Property(x => x.EventType).HasMaxLength(40).IsRequired();
        audits.Property(x => x.Summary).HasMaxLength(512).IsRequired();
        audits.Property(x => x.Notes).HasMaxLength(4000);
        audits.Property(x => x.OccurredAtUtc).HasConversion(x => x, x => DateTime.SpecifyKind(x, DateTimeKind.Utc));
        modelBuilder.Entity<Technician>().Property(x => x.Name).HasMaxLength(80).IsRequired();
        modelBuilder.Entity<Technician>().Property(x => x.Specialty).HasMaxLength(100).IsRequired();
    }
}

// Tooling creates migrations without starting the web host or modifying demo data.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ForgeOpsDbContext>
{
    public ForgeOpsDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<ForgeOpsDbContext>().UseSqlite("Data Source=forgeops-design.db").Options);
}
