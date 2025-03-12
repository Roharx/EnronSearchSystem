using Microsoft.EntityFrameworkCore;
using Database.Models;
using System;

namespace Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public AppDbContext() { }

    public DbSet<Word> Words { get; set; }
    public DbSet<FileMetadata> Files { get; set; }
    public DbSet<Occurrence> Occurrences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Word Table
        modelBuilder.Entity<Word>()
            .HasKey(w => w.WordId);

        // FileMetadata Table
        modelBuilder.Entity<FileMetadata>()
            .HasKey(f => f.FileId);

        // Occurrence Table (Many-to-Many)
        modelBuilder.Entity<Occurrence>()
            .HasKey(o => new { o.WordId, o.FileId });

        modelBuilder.Entity<Occurrence>()
            .HasOne(o => o.Word)
            .WithMany()
            .HasForeignKey(o => o.WordId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Occurrence>()
            .HasOne(o => o.File)
            .WithMany()
            .HasForeignKey(o => o.FileId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            if (string.IsNullOrEmpty(databaseUrl))
            {
                throw new InvalidOperationException("DATABASE_URL environment variable is not set.");
            }

            var uri = new Uri(databaseUrl);
            var host = uri.Host;
            var port = uri.Port;
            var dbName = uri.AbsolutePath.TrimStart('/');
            var userInfo = uri.UserInfo.Split(':');
            var username = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : "";

            var connectionString = $"Host={host};Port={port};Database={dbName};Username={username};Password={password};";

            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}