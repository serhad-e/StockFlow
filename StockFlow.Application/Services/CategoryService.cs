using System.Globalization;
using Microsoft.EntityFrameworkCore;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;
using StockFlow.Infrastructure.Persistence;

namespace StockFlow.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => !c.IsDeleted)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name, // İsimler zaten kayıt anında Title Case yapıldı
                Description = c.Description
            }).ToListAsync();
    }

    public async Task<int> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            // Disiplin: "gIDA" -> "Gıda"
            Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dto.Name.ToLower()),
            Description = !string.IsNullOrEmpty(dto.Description) 
                ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dto.Description.ToLower()) 
                : dto.Description,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category.Id;
    }

    public async Task<bool> UpdateCategoryAsync(int id, CreateCategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null || category.IsDeleted) return false;

        category.Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dto.Name.ToLower());
        category.Description = dto.Description;
        category.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;

        category.IsDeleted = true;
        category.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}