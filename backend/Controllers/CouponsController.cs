using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly AppDbContext _db;
    public CouponsController(AppDbContext db) => _db = db;

    [HttpPost("validate")]
    public async Task<ActionResult<CouponValidationResult>> ValidateCoupon(ValidateCouponDto dto)
    {
        var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == dto.Code);
        if (coupon == null)
            return Ok(new CouponValidationResult(false, "Invalid coupon code", 0, 0));

        if (!coupon.IsActive)
            return Ok(new CouponValidationResult(false, "Coupon is inactive", 0, 0));

        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt < DateTime.UtcNow)
            return Ok(new CouponValidationResult(false, "Coupon has expired", 0, 0));

        if (coupon.TimesUsed >= coupon.UsageLimit)
            return Ok(new CouponValidationResult(false, "Coupon usage limit reached", 0, 0));

        if (coupon.MinOrderAmount.HasValue && dto.OrderTotal < coupon.MinOrderAmount)
            return Ok(new CouponValidationResult(false, $"Minimum order amount is ${coupon.MinOrderAmount}", 0, 0));

        var discount = dto.OrderTotal * coupon.DiscountPercent / 100;
        if (coupon.MaxDiscount.HasValue && discount > coupon.MaxDiscount.Value)
            discount = coupon.MaxDiscount.Value;

        return Ok(new CouponValidationResult(true, $"{coupon.DiscountPercent}% discount applied!", coupon.DiscountPercent, discount));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<CouponDto>>> GetCoupons()
    {
        var coupons = await _db.Coupons.OrderByDescending(c => c.CreatedAt)
            .Select(c => new CouponDto(c.Id, c.Code, c.DiscountPercent, c.MaxDiscount, c.MinOrderAmount,
                c.IsActive, c.UsageLimit, c.TimesUsed, c.ExpiresAt))
            .ToListAsync();
        return Ok(coupons);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CreateCoupon(CreateCouponDto dto)
    {
        if (await _db.Coupons.AnyAsync(c => c.Code == dto.Code))
            return BadRequest(new { message = "Coupon code already exists" });

        var coupon = new Coupon
        {
            Code = dto.Code.ToUpper(),
            DiscountPercent = dto.DiscountPercent,
            MaxDiscount = dto.MaxDiscount,
            MinOrderAmount = dto.MinOrderAmount,
            UsageLimit = dto.UsageLimit,
            ExpiresAt = dto.ExpiresAt
        };

        _db.Coupons.Add(coupon);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Coupon created", coupon.Id });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteCoupon(int id)
    {
        var coupon = await _db.Coupons.FindAsync(id);
        if (coupon == null) return NotFound();
        _db.Coupons.Remove(coupon);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Coupon deleted" });
    }
}
