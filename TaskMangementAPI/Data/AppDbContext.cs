using Microsoft.EntityFrameworkCore;
using TaskMangementAPI.Models;

namespace TaskMangementAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTask> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>()
            .HasIndex(p => p.Name)
            .IsUnique(); 

        modelBuilder.Entity<Project>()
            .HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade); 

        modelBuilder.Entity<ProjectTask>()
            .Property(t => t.Status)
            .HasConversion<string>();

        modelBuilder.Entity<ProjectTask>()
            .Property(t => t.Priority)
            .HasConversion<string>();
    }
}