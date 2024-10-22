using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalesOrders.Models;

namespace SalesOrders.Services
{
    // Change the base class to IdentityDbContext<IdentityUser>
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product>? Products { get; set; }
        public DbSet<Order>? Orders { get; set; }
        public DbSet<OrdersProduct>? OrdersProducts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Ensure base.OnModelCreating is called for Identity

            // Define the composite primary key for OrdersProduct
            modelBuilder.Entity<OrdersProduct>()
                .HasKey(op => new { op.OrderId, op.ProductId });

            // Define the relationship between Order and OrdersProduct
            modelBuilder.Entity<OrdersProduct>()
                .HasOne(op => op.Order)
                .WithMany(o => o.OrdersProducts)
                .HasForeignKey(op => op.OrderId);

            // Define the relationship between Product and OrdersProduct
            modelBuilder.Entity<OrdersProduct>()
                .HasOne(op => op.Product)
                .WithMany(p => p.OrderProducts)
                .HasForeignKey(op => op.ProductId);

            // Specify precision and scale for decimal properties
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)"); // Set precision and scale for TotalAmount
        }
    }
}