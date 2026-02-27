using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistory => Set<OrderStatusHistory>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Username).IsUnique();
        });

        // ── Product ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Price).HasColumnType("decimal(18,2)");
            e.Property(p => p.OldPrice).HasColumnType("decimal(18,2)");
            e.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId);

            // Required indexes
            e.HasIndex(p => new { p.CategoryId, p.Price, p.IsActive });
            e.HasIndex(p => p.NameNormalized);
            e.HasIndex(p => p.BrandNormalized);
            e.HasIndex(p => p.CreatedAt);
            e.HasIndex(p => p.IsFeatured);
        });

        // ── ProductVariant ──────────────────────────────────────────────────
        modelBuilder.Entity<ProductVariant>(e =>
        {
            e.Property(v => v.Price).HasColumnType("decimal(18,2)");
            e.Property(v => v.RowVersion).IsRowVersion();
            e.HasOne(v => v.Product).WithMany(p => p.Variants).HasForeignKey(v => v.ProductId);

            e.HasIndex(v => new { v.ProductId, v.Size, v.Color, v.Stock });
        });

        // ── ProductImage ────────────────────────────────────────────────────
        modelBuilder.Entity<ProductImage>(e =>
        {
            e.HasOne(pi => pi.Product).WithMany(p => p.Images).HasForeignKey(pi => pi.ProductId);
            e.HasIndex(pi => new { pi.ProductId, pi.DisplayOrder });
        });

        // ── CartItem ────────────────────────────────────────────────────────
        modelBuilder.Entity<CartItem>(e =>
        {
            e.HasOne(ci => ci.User).WithMany(u => u.CartItems).HasForeignKey(ci => ci.UserId);
            e.HasOne(ci => ci.Product).WithMany().HasForeignKey(ci => ci.ProductId);
        });

        // ── WishlistItem ────────────────────────────────────────────────────
        modelBuilder.Entity<WishlistItem>(e =>
        {
            e.HasOne(wi => wi.User).WithMany(u => u.WishlistItems).HasForeignKey(wi => wi.UserId);
            e.HasOne(wi => wi.Product).WithMany().HasForeignKey(wi => wi.ProductId);
            e.HasIndex(wi => new { wi.UserId, wi.ProductId }).IsUnique();
        });

        // ── Review ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Review>(e =>
        {
            e.HasOne(r => r.User).WithMany(u => u.Reviews).HasForeignKey(r => r.UserId);
            e.HasOne(r => r.Product).WithMany(p => p.Reviews).HasForeignKey(r => r.ProductId);
            e.HasIndex(r => new { r.UserId, r.ProductId }).IsUnique();
            e.HasIndex(r => new { r.ProductId, r.Status, r.CreatedAt });
        });

        // ── Order ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Order>(e =>
        {
            e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.DiscountAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.ShippingCost).HasColumnType("decimal(18,2)");
            e.Property(o => o.TaxAmount).HasColumnType("decimal(18,2)");
            e.HasOne(o => o.User).WithMany(u => u.Orders).HasForeignKey(o => o.UserId);

            e.HasIndex(o => new { o.UserId, o.CreatedAt, o.Status });
            e.HasIndex(o => o.IdempotencyKey).IsUnique()
                .HasFilter("[IdempotencyKey] IS NOT NULL");
        });

        // ── OrderItem ───────────────────────────────────────────────────────
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.Property(oi => oi.Price).HasColumnType("decimal(18,2)");
            e.HasOne(oi => oi.Order).WithMany(o => o.OrderItems).HasForeignKey(oi => oi.OrderId);
            e.HasOne(oi => oi.Product).WithMany(p => p.OrderItems).HasForeignKey(oi => oi.ProductId);
        });

        // ── OrderStatusHistory ──────────────────────────────────────────────
        modelBuilder.Entity<OrderStatusHistory>(e =>
        {
            e.HasOne(h => h.Order).WithMany(o => o.StatusHistory).HasForeignKey(h => h.OrderId);
            e.HasIndex(h => new { h.OrderId, h.CreatedAt });
        });

        // ── Coupon ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Coupon>(e =>
        {
            e.HasIndex(c => c.Code).IsUnique();
            e.Property(c => c.MaxDiscount).HasColumnType("decimal(18,2)");
            e.Property(c => c.MinOrderAmount).HasColumnType("decimal(18,2)");
            e.HasIndex(c => new { c.IsActive, c.ExpiresAt });
        });

        // ── RefreshToken ────────────────────────────────────────────────────
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasOne(rt => rt.User).WithMany(u => u.RefreshTokens).HasForeignKey(rt => rt.UserId);
            e.HasIndex(rt => rt.Token).IsUnique();
        });

        // ── Address ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Address>(e =>
        {
            e.HasOne(a => a.User).WithMany(u => u.Addresses).HasForeignKey(a => a.UserId);
        });

        // ── StockReservation ────────────────────────────────────────────────
        modelBuilder.Entity<StockReservation>(e =>
        {
            e.HasOne(sr => sr.Order).WithMany(o => o.StockReservations).HasForeignKey(sr => sr.OrderId);
            e.HasOne(sr => sr.ProductVariant).WithMany().HasForeignKey(sr => sr.ProductVariantId);
            e.HasIndex(sr => new { sr.Status, sr.ExpiresAt });
        });

        // ── WebhookEvent ────────────────────────────────────────────────────
        modelBuilder.Entity<WebhookEvent>(e =>
        {
            e.HasIndex(we => new { we.Provider, we.EventId }).IsUnique();
        });

        // ── IdempotencyRecord ───────────────────────────────────────────────
        modelBuilder.Entity<IdempotencyRecord>(e =>
        {
            e.HasIndex(ir => ir.Key).IsUnique();
            e.HasIndex(ir => ir.ExpiresAt);
        });
    }
}
