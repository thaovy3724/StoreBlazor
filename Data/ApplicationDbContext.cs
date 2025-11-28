using Microsoft.EntityFrameworkCore;
using StoreBlazor.Models;

namespace StoreBlazor.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Map kiểu enum (int) -> string (database)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Product ↔ Inventory : 1-1
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Inventory)
                .WithOne(i => i.Product)
                .HasForeignKey<Inventory>(i => i.ProductId);

            // USER.role
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasColumnName("role");

            // ORDER.status
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(10)
                .HasColumnName("status");

            // PAYMENT.payment_method
            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentMethod)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasColumnName("payment_method");

            // PROMOTION.discount_type
            modelBuilder.Entity<Promotion>()
                .Property(p => p.DiscountType)
                .HasConversion<string>()
                .HasMaxLength(10)
                .HasColumnName("discount_type");

            // PROMOTION.status
            modelBuilder.Entity<Promotion>()
                .Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(10)
                .HasColumnName("status");

            // Supplier ↔ Product : 1 - N
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);


            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
    }
}
