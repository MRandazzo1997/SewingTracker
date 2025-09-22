using Microsoft.EntityFrameworkCore;
using SewingTracker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SewingTracker.Data
{
    public class TailorDbContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Cloth> Cloths { get; set; }
        public DbSet<WorkRecord> WorkRecords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TailorShopMonitor",
                "database.db"
            );

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.EmployeeBarcode).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EmployeeBarcode).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<Cloth>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.ReceiptBarcode).IsUnique();
                entity.Property(c => c.ClothId).IsRequired().HasMaxLength(50);
                entity.Property(c => c.ReceiptBarcode).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Price).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<WorkRecord>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.HasOne(w => w.Employee)
                      .WithMany()
                      .HasForeignKey(w => w.EmployeeId);
                entity.HasOne(w => w.Cloth)
                      .WithMany()
                      .HasForeignKey(w => w.ClothId);
            });
        }
    }
}
