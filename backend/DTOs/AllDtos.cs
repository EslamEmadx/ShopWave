namespace backend.DTOs;

// Auth DTOs
public record RegisterDto(string Username, string Email, string Password);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string AccessToken, string RefreshToken, string Username, string Email, string Role, int UserId);
public record UpdateProfileDto(string? Username, string? Phone, string? Address, string? City);
public record RefreshTokenDto(string RefreshToken);
public record RevokeTokenDto(string RefreshToken);
public record AddressDto(int Id, string Label, string Street, string City, string State, string ZipCode, string Country, string Phone, bool IsDefault);
public record CreateAddressDto(string Label, string Street, string City, string State, string ZipCode, string Country, string Phone, bool IsDefault);

// Product DTOs
public record ProductDto(int Id, string Name, string Description, decimal Price, decimal? OldPrice,
    string ImageUrl, int Stock, double Rating, int ReviewCount, bool IsFeatured, int CategoryId, string CategoryName);
public record CreateProductDto(string Name, string Description, decimal Price, decimal? OldPrice,
    string ImageUrl, int Stock, bool IsFeatured, int CategoryId);
public record UpdateProductDto(string? Name, string? Description, decimal? Price, decimal? OldPrice,
    string? ImageUrl, int? Stock, bool? IsFeatured, int? CategoryId);

// Category DTOs
public record CategoryDto(int Id, string Name, string Description, string ImageUrl, int ProductCount);
public record CreateCategoryDto(string Name, string Description, string ImageUrl);

// Cart DTOs
public record CartItemDto(int Id, int ProductId, string ProductName, string ProductImage, decimal Price, int Quantity, int Stock);
public record AddToCartDto(int ProductId, int Quantity = 1);
public record UpdateCartDto(int Quantity);

// Wishlist DTOs
public record WishlistItemDto(int Id, int ProductId, string ProductName, string ProductImage, decimal Price, int Stock);

// Review DTOs
public record ReviewDto(int Id, int Rating, string Comment, DateTime CreatedAt, string Username, int UserId);
public record CreateReviewDto(int ProductId, int Rating, string Comment);

// Order DTOs
public record OrderDto(int Id, decimal TotalAmount, string Status, string PaymentStatus, string ShippingAddress,
    string ShippingCity, string Phone, string? CouponCode, decimal DiscountAmount, DateTime CreatedAt, List<OrderItemDto> Items);
public record OrderItemDto(int Id, int ProductId, string ProductName, string ProductImage, decimal Price, int Quantity);
public record CreateOrderDto(string ShippingAddress, string ShippingCity, string Phone, string? CouponCode);
public record UpdateOrderStatusDto(string Status);

// Coupon DTOs
public record CouponDto(int Id, string Code, int DiscountPercent, decimal? MaxDiscount, decimal? MinOrderAmount,
    bool IsActive, int UsageLimit, int TimesUsed, DateTime? ExpiresAt);
public record CreateCouponDto(string Code, int DiscountPercent, decimal? MaxDiscount, decimal? MinOrderAmount,
    int UsageLimit, DateTime? ExpiresAt);
public record ValidateCouponDto(string Code, decimal OrderTotal);
public record CouponValidationResult(bool IsValid, string? Message, int DiscountPercent, decimal DiscountAmount);

// Payment DTOs
public record CreateCheckoutDto(int OrderId);
public record CheckoutSessionDto(string SessionId, string Url);

// Admin DTOs
public record DashboardDto(decimal TotalRevenue, int TotalOrders, int TotalUsers, int TotalProducts,
    List<RecentOrderDto> RecentOrders, List<TopProductDto> TopProducts, List<MonthlySalesDto> MonthlySales);
public record RecentOrderDto(int Id, string Username, decimal Total, string Status, DateTime Date);
public record TopProductDto(int Id, string Name, string ImageUrl, int TotalSold, decimal Revenue);
public record MonthlySalesDto(string Month, decimal Revenue, int Orders);
