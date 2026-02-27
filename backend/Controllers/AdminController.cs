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
        int totalOrders = 0, totalUsers = 0, totalProducts = 0;
        try
        {
            totalRevenue = await _db.Orders
                .Where(o => o.PaymentStatus == "Paid" || o.Status != "Cancelled")
                .SumAsync(o => o.TotalAmount);
            totalOrders = await _db.Orders.CountAsync();
            totalUsers  = await _db.Users.CountAsync();
            totalProducts = await _db.Products.CountAsync();
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
                .Select(o => new RecentOrderDto(o.Id, o.User.Username, o.TotalAmount, o.Status, o.CreatedAt))
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

        return Ok(new DashboardDto(totalRevenue, totalOrders, totalUsers, totalProducts,
            recentOrders, topProducts, monthlySales));
    }
}
