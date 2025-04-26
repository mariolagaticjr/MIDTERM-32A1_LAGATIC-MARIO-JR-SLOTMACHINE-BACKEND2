using SlotMachineApi.Models;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;

namespace SlotMachineApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<GameResult> GameResults { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GameResult>()
                .HasOne(g => g.Player)
                .WithMany()
                .HasForeignKey(g => g.StudentNumber);

            modelBuilder.Entity<Player>()
                .Property(p => p.StudentNumber)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<Player>()
                .HasIndex(p => p.StudentNumber)
                .IsUnique();

            modelBuilder.Entity<Player>()
                .Property(p => p.FirstName)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Player>()
                .Property(p => p.LastName)
                .HasMaxLength(50)
                .IsRequired();
        }
    }
}
