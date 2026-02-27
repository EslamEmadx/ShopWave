using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
    {
        var categories = await _db.Categories
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.ImageUrl, c.Products.Count))
            .ToListAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var c = await _db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        if (c == null) return NotFound();
        return Ok(new CategoryDto(c.Id, c.Name, c.Description, c.ImageUrl, c.Products.Count));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CreateCategory(CreateCategoryDto dto)
    {
        var category = new Category { Name = dto.Name, Description = dto.Description, ImageUrl = dto.ImageUrl };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id },
            new CategoryDto(category.Id, category.Name, category.Description, category.ImageUrl, 0));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateCategory(int id, CreateCategoryDto dto)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();
        category.Name = dto.Name;
        category.Description = dto.Description;
        category.ImageUrl = dto.ImageUrl;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Category updated" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteCategory(int id)
    {
        var category = await _db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return NotFound();
        if (category.Products.Any()) return BadRequest(new { message = "Cannot delete category with products" });
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Category deleted" });
    }
}
