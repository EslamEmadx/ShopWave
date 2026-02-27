using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Price).HasColumnType("decimal(18,2)");
            e.Property(p => p.OldPrice).HasColumnType("decimal(18,2)");
            e.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId);
        });

        modelBuilder.Entity<CartItem>(e =>
        {
            e.HasOne(ci => ci.User).WithMany(u => u.CartItems).HasForeignKey(ci => ci.UserId);
            e.HasOne(ci => ci.Product).WithMany().HasForeignKey(ci => ci.ProductId);
        });

        modelBuilder.Entity<WishlistItem>(e =>
        {
            e.HasOne(wi => wi.User).WithMany(u => u.WishlistItems).HasForeignKey(wi => wi.UserId);
            e.HasOne(wi => wi.Product).WithMany().HasForeignKey(wi => wi.ProductId);
            e.HasIndex(wi => new { wi.UserId, wi.ProductId }).IsUnique();
        });

        modelBuilder.Entity<Review>(e =>
        {
            e.HasOne(r => r.User).WithMany(u => u.Reviews).HasForeignKey(r => r.UserId);
            e.HasOne(r => r.Product).WithMany(p => p.Reviews).HasForeignKey(r => r.ProductId);
            e.HasIndex(r => new { r.UserId, r.ProductId }).IsUnique();
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.DiscountAmount).HasColumnType("decimal(18,2)");
            e.HasOne(o => o.User).WithMany(u => u.Orders).HasForeignKey(o => o.UserId);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.Property(oi => oi.Price).HasColumnType("decimal(18,2)");
            e.HasOne(oi => oi.Order).WithMany(o => o.OrderItems).HasForeignKey(oi => oi.OrderId);
            e.HasOne(oi => oi.Product).WithMany(p => p.OrderItems).HasForeignKey(oi => oi.ProductId);
        });

        modelBuilder.Entity<Coupon>(e =>
        {
            e.HasIndex(c => c.Code).IsUnique();
            e.Property(c => c.MaxDiscount).HasColumnType("decimal(18,2)");
            e.Property(c => c.MinOrderAmount).HasColumnType("decimal(18,2)");
        });
    }
}
