using backend.Models;
using BCrypt.Net;

namespace backend.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext context)
    {
        if (context.Products.Any()) return;

        // Admin user
        var admin = new User
        {
            Username = "admin",
            Email = "admin@shopwave.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = "Admin",
            Phone = "01000000000",
            Address = "Cairo, Egypt"
        };
        context.Users.Add(admin);

        // Categories
        var categories = new List<Category>
        {
            new() { Name = "Electronics", Description = "Latest gadgets & electronics", ImageUrl = "https://images.unsplash.com/photo-1498049794561-7780e7231661?w=400" },
            new() { Name = "Fashion", Description = "Trendy clothing & accessories", ImageUrl = "https://images.unsplash.com/photo-1445205170230-053b83016050?w=400" },
            new() { Name = "Home & Living", Description = "Furniture & home decor", ImageUrl = "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=400" },
            new() { Name = "Sports", Description = "Sports equipment & activewear", ImageUrl = "https://images.unsplash.com/photo-1461896836934-bd45ba8fcf9b?w=400" },
            new() { Name = "Books", Description = "Best sellers & new releases", ImageUrl = "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?w=400" },
            new() { Name = "Beauty", Description = "Skincare, makeup & fragrances", ImageUrl = "https://images.unsplash.com/photo-1596462502278-27bfdc403348?w=400" }
        };
        context.Categories.AddRange(categories);
        context.SaveChanges();

        // Products
        var products = new List<Product>
        {
            // Electronics
            new() { Name = "Wireless Headphones Pro", Description = "Premium noise-cancelling wireless headphones with 40hr battery life. Features adaptive ANC, Hi-Res audio, and multipoint connection.", Price = 299.99m, OldPrice = 399.99m, ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400", Stock = 50, Rating = 4.8, ReviewCount = 124, IsFeatured = true, CategoryId = categories[0].Id },
            new() { Name = "Smart Watch Ultra", Description = "Advanced smartwatch with health monitoring, GPS, and 7-day battery. Water-resistant to 100m.", Price = 449.99m, ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400", Stock = 35, Rating = 4.6, ReviewCount = 89, IsFeatured = true, CategoryId = categories[0].Id },
            new() { Name = "4K Webcam Stream", Description = "Professional 4K webcam with auto-focus, noise-reducing microphone, and ring light.", Price = 129.99m, OldPrice = 179.99m, ImageUrl = "https://images.unsplash.com/photo-1587826080692-f439cd0b70da?w=400", Stock = 80, Rating = 4.4, ReviewCount = 56, CategoryId = categories[0].Id },

            // Fashion
            new() { Name = "Classic Leather Jacket", Description = "Genuine leather jacket with a timeless design. Perfect for casual and semi-formal occasions.", Price = 189.99m, OldPrice = 249.99m, ImageUrl = "https://images.unsplash.com/photo-1551028719-00167b16eac5?w=400", Stock = 25, Rating = 4.7, ReviewCount = 67, IsFeatured = true, CategoryId = categories[1].Id },
            new() { Name = "Running Sneakers Air", Description = "Lightweight running shoes with responsive cushioning and breathable mesh upper.", Price = 129.99m, ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400", Stock = 100, Rating = 4.5, ReviewCount = 203, IsFeatured = true, CategoryId = categories[1].Id },
            new() { Name = "Designer Sunglasses", Description = "UV400 polarized sunglasses with titanium frame. Includes premium carrying case.", Price = 159.99m, OldPrice = 199.99m, ImageUrl = "https://images.unsplash.com/photo-1572635196237-14b3f281503f?w=400", Stock = 60, Rating = 4.3, ReviewCount = 45, CategoryId = categories[1].Id },

            // Home & Living
            new() { Name = "Minimalist Desk Lamp", Description = "Modern LED desk lamp with adjustable brightness, color temperature, and wireless charging base.", Price = 79.99m, ImageUrl = "https://images.unsplash.com/photo-1507473885765-e6ed057ab6fe?w=400", Stock = 45, Rating = 4.6, ReviewCount = 78, CategoryId = categories[2].Id },
            new() { Name = "Luxury Throw Blanket", Description = "Ultra-soft cashmere blend throw blanket. Machine washable and hypoallergenic.", Price = 89.99m, OldPrice = 119.99m, ImageUrl = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=400", Stock = 70, Rating = 4.8, ReviewCount = 156, IsFeatured = true, CategoryId = categories[2].Id },
            new() { Name = "Ceramic Vase Set", Description = "Set of 3 handcrafted ceramic vases in matte finish. Perfect for modern home decor.", Price = 59.99m, ImageUrl = "https://images.unsplash.com/photo-1578500494198-246f612d3b3d?w=400", Stock = 40, Rating = 4.4, ReviewCount = 34, CategoryId = categories[2].Id },

            // Sports
            new() { Name = "Yoga Mat Premium", Description = "Extra thick non-slip yoga mat with alignment lines. Includes carrying strap.", Price = 49.99m, ImageUrl = "https://images.unsplash.com/photo-1601925260368-ae2f83cf8b7f?w=400", Stock = 90, Rating = 4.7, ReviewCount = 189, CategoryId = categories[3].Id },
            new() { Name = "Fitness Tracker Band", Description = "Waterproof fitness tracker with heart rate monitor, sleep tracking, and 14-day battery.", Price = 69.99m, OldPrice = 99.99m, ImageUrl = "https://images.unsplash.com/photo-1575311373937-040b8e1fd5b6?w=400", Stock = 120, Rating = 4.3, ReviewCount = 234, CategoryId = categories[3].Id },
            new() { Name = "Stainless Steel Water Bottle", Description = "Double-wall insulated water bottle. Keeps drinks cold 24hrs or hot 12hrs. BPA-free.", Price = 34.99m, ImageUrl = "https://images.unsplash.com/photo-1602143407151-7111542de6e8?w=400", Stock = 200, Rating = 4.9, ReviewCount = 312, IsFeatured = true, CategoryId = categories[3].Id },

            // Books
            new() { Name = "The Art of Programming", Description = "Comprehensive guide to modern software development practices and design patterns.", Price = 39.99m, ImageUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400", Stock = 150, Rating = 4.8, ReviewCount = 89, CategoryId = categories[4].Id },
            new() { Name = "Mindset: Growth Edition", Description = "Bestselling book on developing a growth mindset for success in work and life.", Price = 24.99m, OldPrice = 34.99m, ImageUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400", Stock = 200, Rating = 4.6, ReviewCount = 445, CategoryId = categories[4].Id },
            new() { Name = "Digital Photography Guide", Description = "Master photography from basics to advanced techniques with stunning visual examples.", Price = 44.99m, ImageUrl = "https://images.unsplash.com/photo-1553729784-e91953dec042?w=400", Stock = 75, Rating = 4.5, ReviewCount = 67, CategoryId = categories[4].Id },

            // Beauty
            new() { Name = "Vitamin C Serum", Description = "Advanced vitamin C serum with hyaluronic acid for brightening and anti-aging. Dermatologist tested.", Price = 29.99m, ImageUrl = "https://images.unsplash.com/photo-1620916566398-39f1143ab7be?w=400", Stock = 180, Rating = 4.7, ReviewCount = 278, CategoryId = categories[5].Id },
            new() { Name = "Luxury Perfume Collection", Description = "Eau de parfum with notes of jasmine, sandalwood, and vanilla. Long-lasting 12hr formula.", Price = 119.99m, OldPrice = 159.99m, ImageUrl = "https://images.unsplash.com/photo-1541643600914-78b084683601?w=400", Stock = 40, Rating = 4.8, ReviewCount = 156, IsFeatured = true, CategoryId = categories[5].Id },
            new() { Name = "Makeup Brush Set Pro", Description = "Professional 12-piece makeup brush set with synthetic fibers and leather case.", Price = 49.99m, ImageUrl = "https://images.unsplash.com/photo-1596462502278-27bfdc403348?w=400", Stock = 65, Rating = 4.5, ReviewCount = 123, CategoryId = categories[5].Id }
        };
        context.Products.AddRange(products);

        // Coupons
        var coupons = new List<Coupon>
        {
            new() { Code = "WELCOME10", DiscountPercent = 10, MinOrderAmount = 50, UsageLimit = 1000, ExpiresAt = DateTime.UtcNow.AddMonths(6) },
            new() { Code = "SAVE20", DiscountPercent = 20, MaxDiscount = 100, MinOrderAmount = 100, UsageLimit = 500, ExpiresAt = DateTime.UtcNow.AddMonths(3) },
            new() { Code = "MEGA30", DiscountPercent = 30, MaxDiscount = 150, MinOrderAmount = 200, UsageLimit = 100, ExpiresAt = DateTime.UtcNow.AddMonths(1) }
        };
        context.Coupons.AddRange(coupons);

        context.SaveChanges();
    }
}
