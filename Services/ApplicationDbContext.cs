using Microsoft.EntityFrameworkCore;
using SalesOrders.Models;

namespace SalesOrders.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define the composite primary key for OrderProduct
            modelBuilder.Entity<OrderProduct>()
                .HasKey(op => new { op.OrderId, op.ProductId });

            // Define the relationship between Order and OrderProduct
            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.Order)
                .WithMany(o => o.OrderProducts)
                .HasForeignKey(op => op.OrderId);

            // Define the relationship between Product and OrderProduct
            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.Product)
                .WithMany(p => p.OrderProducts)
                .HasForeignKey(op => op.ProductId);

            // Specify precision and scale for decimal properties
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)"); // Set precision and scale as needed

            modelBuilder.Entity<OrderProduct>()
                .Property(op => op.UnitPrice)
                .HasColumnType("decimal(18,2)"); // Set precision and scale as needed

            base.OnModelCreating(modelBuilder);
        }
    }
}
