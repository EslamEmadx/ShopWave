using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Models;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    public OrdersController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<ActionResult> PlaceOrder(CreateOrderDto dto)
    {
        var userId = GetUserId();
        var cartItems = await _db.CartItems
            .Where(ci => ci.UserId == userId)
            .Include(ci => ci.Product)
            .ToListAsync();

        if (!cartItems.Any()) return BadRequest(new { message = "Cart is empty" });

        // Stock check
        foreach (var ci in cartItems)
        {
            if (ci.Product.Stock < ci.Quantity)
                return BadRequest(new { message = $"Insufficient stock for product: {ci.Product.Name}" });
        }

        var subtotal = cartItems.Sum(ci => ci.Product.Price * ci.Quantity);
        decimal discountAmount = 0;
        string? couponCode = null;

        if (!string.IsNullOrEmpty(dto.CouponCode))
        {
            var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == dto.CouponCode && c.IsActive);
            if (coupon != null && (coupon.ExpiresAt == null || coupon.ExpiresAt > DateTime.UtcNow)
                && coupon.TimesUsed < coupon.UsageLimit
                && (coupon.MinOrderAmount == null || subtotal >= coupon.MinOrderAmount))
            {
                discountAmount = subtotal * coupon.DiscountPercent / 100;
                if (coupon.MaxDiscount.HasValue && discountAmount > coupon.MaxDiscount.Value)
                    discountAmount = coupon.MaxDiscount.Value;
                couponCode = coupon.Code;
                coupon.TimesUsed++;
            }
        }

        var order = new Order
        {
            UserId = userId,
            TotalAmount = subtotal - discountAmount,
            ShippingAddress = dto.ShippingAddress,
            ShippingCity = dto.ShippingCity,
            Phone = dto.Phone,
            CouponCode = couponCode,
            DiscountAmount = discountAmount,
            Status = "Pending",
            PaymentStatus = "Unpaid",
            PaymentMethod = dto.PaymentMethod,
            OrderItems = cartItems.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                Price = ci.Product.Price
            }).ToList()
        };

        // Reduce stock
        foreach (var ci in cartItems)
        {
            ci.Product.Stock -= ci.Quantity;
        }

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cartItems);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Order placed", orderId = order.Id, total = order.TotalAmount });
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<OrderDto>>> GetOrders(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");

        var query = _db.Orders.AsNoTracking().AsQueryable();
        if (!isAdmin) query = query.Where(o => o.UserId == userId);

        var totalCount = await query.CountAsync();
        var orders = await query.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .Select(o => new OrderDto(o.Id, o.TotalAmount, o.Status, o.PaymentStatus, o.ShippingAddress,
                o.ShippingCity, o.Phone, o.CouponCode, o.DiscountAmount, o.CreatedAt,
                o.OrderItems.Select(oi => new OrderItemDto(oi.Id, oi.ProductId, oi.Product.Name, oi.Product.ImageUrl, oi.Price, oi.Quantity)).ToList()))
            .ToListAsync();

        return Ok(new PaginatedResult<OrderDto>(
            orders, 
            totalCount, 
            page, 
            pageSize, 
            (int)Math.Ceiling((double)totalCount / pageSize)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");

        var o = await _db.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (o == null) return NotFound();
        if (!isAdmin && o.UserId != userId) return Forbid();

        return Ok(new OrderDto(o.Id, o.TotalAmount, o.Status, o.PaymentStatus, o.ShippingAddress,
            o.ShippingCity, o.Phone, o.CouponCode, o.DiscountAmount, o.CreatedAt,
            o.OrderItems.Select(oi => new OrderItemDto(oi.Id, oi.ProductId, oi.Product.Name, oi.Product.ImageUrl, oi.Price, oi.Quantity)).ToList()));
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateStatus(int id, UpdateOrderStatusDto dto)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        // Basic status validation
        var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled", "Refunded" };
        if (!validStatuses.Contains(dto.Status))
            return BadRequest(new { message = "Invalid order status" });

        order.Status = dto.Status;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Status updated" });
    }

    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
