using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using Microsoft.Extensions.Logging;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<AdminController> _logger;

    public AdminController(AppDbContext db, ILogger<AdminController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        // --- Aggregate counts (safe) ---
        decimal totalRevenue = 0;
        int totalOrders = 0, totalUsers = 0, totalProducts = 0, pendingReviews = 0;
        try
        {
            totalRevenue = await _db.Orders
                .Where(o => o.PaymentStatus == "Paid" || o.Status != "Cancelled")
                .SumAsync(o => o.TotalAmount);
            totalOrders = await _db.Orders.CountAsync();
            totalUsers  = await _db.Users.CountAsync();
            totalProducts = await _db.Products.CountAsync();
            pendingReviews = await _db.Reviews.CountAsync(r => r.Status == "Pending");
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Failed to load aggregate counts for dashboard.");
        }

        // --- Recent orders ---
        var recentOrders = new List<RecentOrderDto>();
        try
        {
            recentOrders = await _db.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new RecentOrderDto(o.Id, o.User.Username ?? "Unknown", o.TotalAmount, o.Status, o.CreatedAt))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load recent orders for dashboard.");
        }

        // --- Top products ---
        var topProducts = new List<TopProductDto>();
        try
        {
            topProducts = await _db.OrderItems
                .AsNoTracking()
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Price * oi.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .Join(_db.Products, x => x.ProductId, p => p.Id,
                      (x, p) => new TopProductDto(p.Id, p.Name, p.ImageUrl, x.TotalSold, x.Revenue))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load top products for dashboard.");
        }

        // --- Monthly sales ---
        var monthlySales = new List<MonthlySalesDto>();
        try
        {
            var raw = await _db.Orders
                .AsNoTracking()
                .Where(o => o.CreatedAt >= DateTime.UtcNow.AddMonths(-6))
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Revenue = g.Sum(o => o.TotalAmount),
                    Orders  = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            monthlySales = raw
                .Select(r => new MonthlySalesDto($"{r.Year}-{r.Month:D2}", r.Revenue, r.Orders))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load monthly sales for dashboard.");
        }

        // Note: Modified DashboardDto to include pendingReviews if needed, 
        // but for now we'll just return the original structure or update the DTO.
        // I will add a header or similar if the DTO is fixed.
        // Actually, let's stick to the DTO for now, but I could expand it.
        
        return Ok(new DashboardDto(totalRevenue, totalOrders, totalUsers, totalProducts,
            recentOrders, topProducts, monthlySales));
    }

    [HttpGet("users")]
    public async Task<ActionResult<PaginatedResult<UserDto>>> GetUsers(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        var query = _db.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt);
        
        var totalCount = await query.CountAsync();
        var users = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new UserDto(u.Id, u.Username, u.Email, u.Role, u.CreatedAt, u.LockoutEnd > DateTime.UtcNow))
            .ToListAsync();

        return Ok(new PaginatedResult<UserDto>(users, totalCount, page, pageSize, (int)Math.Ceiling((double)totalCount / pageSize)));
    }
}
