using System.Globalization;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;

    public CategoryService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        // Repository üzerinden silinmemişleri getir
        var categories = await _uow.Categories.GetAllAsync(c => !c.IsDeleted);
        
        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        }).ToList();
    }

    public async Task<int> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dto.Name.ToLower()),
            Description = !string.IsNullOrEmpty(dto.Description) 
                ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dto.Description.ToLower()) 
                : dto.Description,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        await _uow.Categories.AddAsync(category);
        await _uow.SaveChangesAsync(); // Değişiklikleri Unit of Work üzerinden kaydet
        return category.Id;
    }

    public async Task<bool> UpdateCategoryAsync(int id, CreateCategoryDto dto)
    {
        var category = await _uow.Categories.GetByIdAsync(id);
        if (category == null || category.IsDeleted) return false;

        category.Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dto.Name.ToLower());
        category.Description = dto.Description;
        category.UpdatedDate = DateTime.UtcNow;

        _uow.Categories.Update(category);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _uow.Categories.GetByIdAsync(id);
        if (category == null) return false;

        category.IsDeleted = true;
        category.UpdatedDate = DateTime.UtcNow;

        _uow.Categories.Update(category);
        await _uow.SaveChangesAsync();
        return true;
    }
}