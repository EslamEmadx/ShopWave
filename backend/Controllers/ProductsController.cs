using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult> GetProducts(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] string? sort,
        [FromQuery] bool? featured,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var query = _db.Products.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);
        if (featured.HasValue)
            query = query.Where(p => p.IsFeatured == featured);
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice);
        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice);

        query = sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "rating" => query.OrderByDescending(p => p.Rating),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "name" => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var products = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.OldPrice,
                p.ImageUrl, p.Stock, p.Rating, p.ReviewCount, p.IsFeatured, p.CategoryId, p.Category.Name))
            .ToListAsync();

        return Ok(new { products, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var p = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (p == null) return NotFound();

        return Ok(new ProductDto(p.Id, p.Name, p.Description, p.Price, p.OldPrice,
            p.ImageUrl, p.Stock, p.Rating, p.ReviewCount, p.IsFeatured, p.CategoryId, p.Category.Name));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            OldPrice = dto.OldPrice,
            ImageUrl = dto.ImageUrl,
            Stock = dto.Stock,
            IsFeatured = dto.IsFeatured,
            CategoryId = dto.CategoryId
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var p = await _db.Products.Include(p => p.Category).FirstAsync(p => p.Id == product.Id);
        return CreatedAtAction(nameof(GetProduct), new { id = p.Id },
            new ProductDto(p.Id, p.Name, p.Description, p.Price, p.OldPrice,
                p.ImageUrl, p.Stock, p.Rating, p.ReviewCount, p.IsFeatured, p.CategoryId, p.Category.Name));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateProduct(int id, UpdateProductDto dto)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        if (dto.Name != null) product.Name = dto.Name;
        if (dto.Description != null) product.Description = dto.Description;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.OldPrice.HasValue) product.OldPrice = dto.OldPrice.Value;
        if (dto.ImageUrl != null) product.ImageUrl = dto.ImageUrl;
        if (dto.Stock.HasValue) product.Stock = dto.Stock.Value;
        if (dto.IsFeatured.HasValue) product.IsFeatured = dto.IsFeatured.Value;
        if (dto.CategoryId.HasValue) product.CategoryId = dto.CategoryId.Value;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Product updated" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Product deleted" });
    }
}
