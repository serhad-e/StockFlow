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
                Name = c.Name,
                Description = c.Description
            }).ToListAsync();
    }

    public async Task<int> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            CreatedDate = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category.Id;
    }
}