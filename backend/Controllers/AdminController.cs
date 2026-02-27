using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

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
        catch { /* counts default to 0 */ }

        // --- Recent orders ---
        var recentOrders = new List<RecentOrderDto>();
        try
        {
            recentOrders = await _db.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new RecentOrderDto(o.Id, o.User.Username, o.TotalAmount, o.Status, o.CreatedAt))
                .ToListAsync();
        }
        catch { }

        // --- Top products ---
        var topProducts = new List<TopProductDto>();
        try
        {
            topProducts = await _db.OrderItems
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
        catch { }

        // --- Monthly sales (fixed: project to anonymous type in SQL, then map in memory) ---
        var monthlySales = new List<MonthlySalesDto>();
        try
        {
            var raw = await _db.Orders
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
        catch { }

        return Ok(new DashboardDto(totalRevenue, totalOrders, totalUsers, totalProducts,
            recentOrders, topProducts, monthlySales));
    }
}
