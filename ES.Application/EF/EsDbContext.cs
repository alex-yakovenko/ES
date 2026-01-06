using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace ES.Application.EF
{
    public class EsDbContext : DbContext
    {
        public const string ConnectionString =
            "Host=localhost;Port=5432;Database=esdb;Username=sa;Password=MyL0ca!!Post6ges";

        public EsDbContext()
        {
            
        }

        public EsDbContext(DbContextOptions<EsDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=esdb;Username=sa;Password=MyL0ca!!Post6ges",
                options => options.MigrationsHistoryTable("__Migrations"));
        }

        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<ProductReservation> InventoryReservations { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InventoryItem>(entity =>
            {
                entity.ToTable("InventoryItems", "inv");
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductId).IsRequired().HasMaxLength(30);
                entity.Property(e => e.AvailableQuantity).IsRequired();

                entity.HasMany(p => p.Reservations)
                    .WithOne(p => p.InventoryItem)
                    .HasForeignKey(x => x.ProductId);
            });

            modelBuilder.Entity<ProductReservation>(entity =>
            {
                entity.ToTable("InventoryReservations", "inv")
                    .HasKey(e => e.Id);

                entity.Property(p => p.Id)
                    .UseIdentityColumn();

                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.OrderId)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(e => e.CorrelationId)
                    .HasMaxLength(50);
            });


            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders", "ord")
                    .HasKey(p => p.OrderId);

                entity.Property(p => p.OrderId)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(p => p.CustomerId)
                    .HasMaxLength(150);

                entity.HasMany(p => p.Items)
                    .WithOne(p => p.Order)
                    .HasForeignKey(p => p.OrderId);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems", "ord")
                    .HasKey(p => p.Id);

                entity.Property(p => p.Id)
                    .UseIdentityColumn();

                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.OrderId)
                    .IsRequired()
                    .HasMaxLength(30);
            });
        }
    }
}
