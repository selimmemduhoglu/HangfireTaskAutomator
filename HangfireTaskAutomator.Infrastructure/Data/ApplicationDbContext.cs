using HangfireTaskAutomator.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HangfireTaskAutomator.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


    public DbSet<Email> Emails { get; set; } = null!;
    public DbSet<ReportData> ReportDatas { get; set; } = null!;
    public DbSet<TaskHistory> TaskHistorys { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // Email entity konfigürasyonu
        modelBuilder.Entity<Email>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Recipient).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.IsSent).HasDefaultValue(false);
        });

        // ReportData entity konfigürasyonu
        modelBuilder.Entity<ReportData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReportName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ReportType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(255);
            entity.Property(e => e.IsProcessed).HasDefaultValue(false);
        });

        // TaskHistory entity konfigürasyonu
        modelBuilder.Entity<TaskHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.JobId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TaskName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.JobType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
        });



    }




}
