using BstSolutions.Models;
using Microsoft.EntityFrameworkCore;

namespace BstSolutions.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<WorkTask> WorkTasks => Set<WorkTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("ID_Employee");

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.IsActive)
                .IsRequired();

            entity.Property(e => e.CreatedDate)
                .IsRequired();

            entity.HasMany(e => e.WorkTasks)
                .WithOne(t => t.Employee)
                .HasForeignKey(t => t.EmployeeId)
                .HasConstraintName("FK_WorkTasks_Employee")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkTask>(entity =>
        {
            entity.ToTable("WorkTasks");

            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id)
                .HasColumnName("ID_WorkTask");

            entity.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(t => t.Description)
                .HasMaxLength(2000);

            entity.Property(t => t.EmployeeId)
                .HasColumnName("FK_Employee")
                .IsRequired();

            entity.Property(t => t.Priority)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(t => t.Status)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(t => t.DueDate)
                .IsRequired();

            entity.Property(t => t.CreatedDate)
                .IsRequired();

            entity.Property(t => t.CompletedDate);

            entity.HasIndex(t => t.EmployeeId)
                .HasDatabaseName("IX_WorkTasks_FK_Employee");
            entity.HasIndex(t => t.Status)
                .HasDatabaseName("IX_WorkTasks_Status");
            entity.HasIndex(t => t.Priority)
                .HasDatabaseName("IX_WorkTasks_Priority");
            entity.HasIndex(t => t.DueDate)
                .HasDatabaseName("IX_WorkTasks_DueDate");
        });
    }
}
