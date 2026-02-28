using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

// Generic Paged Results
public record PaginatedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize, int TotalPages);

// Auth DTOs
public record RegisterDto(
    [Required, MinLength(3)] string Username, 
    [Required, EmailAddress] string Email, 
    [Required, MinLength(6)] string Password);

public record LoginDto(
    [Required, EmailAddress] string Email, 
    [Required] string Password);

public record AuthResponseDto(string AccessToken, string RefreshToken, string Username, string Email, string Role, int UserId);

public record UpdateProfileDto(string? Username, string? Phone, string? Address, string? City);

public record RefreshTokenDto([Required] string RefreshToken);

public record RevokeTokenDto([Required] string RefreshToken);

public record AddressDto(int Id, string Label, string Street, string City, string State, string ZipCode, string Country, string Phone, bool IsDefault);

public record CreateAddressDto(
    [Required] string Label, 
    [Required] string Street, 
    [Required] string City, 
    string? State, 
    [Required] string ZipCode, 
    [Required] string Country, 
    [Required, Phone] string Phone, 
    bool IsDefault);

// Product DTOs
public record ProductDto(int Id, string Name, string Description, decimal Price, decimal? OldPrice,
    string ImageUrl, int Stock, decimal RatingAvg, int ReviewCount, bool IsFeatured, int CategoryId, string CategoryName);

public record CreateProductDto(
    [Required, MinLength(3)] string Name, 
    [Required] string Description, 
    [Range(0.01, 1000000)] decimal Price, 
    decimal? OldPrice,
    [Required, Url] string ImageUrl, 
    [Range(0, 100000)] int Stock, 
    bool IsFeatured, 
    int CategoryId);

public record UpdateProductDto(string? Name, string? Description, decimal? Price, decimal? OldPrice,
    string? ImageUrl, int? Stock, bool? IsFeatured, int? CategoryId);

// Category DTOs
public record CategoryDto(int Id, string Name, string Description, string ImageUrl, int ProductCount);

public record CreateCategoryDto(
    [Required] string Name, 
    [Required] string Description, 
    [Required, Url] string ImageUrl);

// Cart DTOs
public record CartItemDto(int Id, int ProductId, string ProductName, string ProductImage, decimal Price, int Quantity, int Stock);

public record AddToCartDto(int ProductId, [Range(1, 100)] int Quantity = 1);

public record UpdateCartDto([Range(1, 100)] int Quantity);

// Wishlist DTOs
public record WishlistItemDto(int Id, int ProductId, string ProductName, string ProductImage, decimal Price, int Stock);

// Review DTOs
public record ReviewDto(int Id, int Rating, string Comment, DateTime CreatedAt, string Username, int UserId, string Status, bool IsVerified);

public record CreateReviewDto(int ProductId, [Range(1, 5)] int Rating, [Required, MinLength(5)] string Comment);

// Order DTOs
public record OrderDto(int Id, decimal TotalAmount, string Status, string PaymentStatus, string ShippingAddress,
    string ShippingCity, string Phone, string? CouponCode, decimal DiscountAmount, DateTime CreatedAt, List<OrderItemDto> Items);

public record OrderItemDto(int Id, int ProductId, string ProductName, string ProductImage, decimal Price, int Quantity);

public record CreateOrderDto(
    [Required] string ShippingAddress, 
    [Required] string ShippingCity, 
    [Required, Phone] string Phone, 
    string? CouponCode,
    [Required] string PaymentMethod = "Stripe");

public record UpdateOrderStatusDto([Required] string Status);

// Coupon DTOs
public record CouponDto(int Id, string Code, int DiscountPercent, decimal? MaxDiscount, decimal? MinOrderAmount,
    bool IsActive, int UsageLimit, int TimesUsed, DateTime? ExpiresAt);

public record CreateCouponDto(
    [Required, MinLength(3)] string Code, 
    [Range(1, 100)] int DiscountPercent, 
    decimal? MaxDiscount, 
    decimal? MinOrderAmount,
    [Range(1, 1000000)] int UsageLimit, 
    DateTime? ExpiresAt);

public record UpdateCouponDto(string? Code, int? DiscountPercent, decimal? MaxDiscount, decimal? MinOrderAmount,
    bool? IsActive, int? UsageLimit, DateTime? ExpiresAt);

public record ValidateCouponDto([Required] string Code, [Range(0, 1000000)] decimal OrderTotal);

public record CouponValidationResult(bool IsValid, string? Message, int DiscountPercent, decimal DiscountAmount);

// Payment DTOs
public record CreateCheckoutDto([Required] int OrderId);

public record CheckoutSessionDto(string SessionId, string Url);

// Admin DTOs
public record DashboardDto(decimal TotalRevenue, int TotalOrders, int TotalUsers, int TotalProducts,
    List<RecentOrderDto> RecentOrders, List<TopProductDto> TopProducts, List<MonthlySalesDto> MonthlySales);

public record RecentOrderDto(int Id, string Username, decimal Total, string Status, DateTime Date);

public record TopProductDto(int Id, string Name, string ImageUrl, int TotalSold, decimal Revenue);

public record MonthlySalesDto(string Month, decimal Revenue, int Orders);

public record UserDto(int Id, string Username, string Email, string Role, DateTime CreatedAt, bool IsLockedOut);
