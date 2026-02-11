using StockFlow.Application.DTOs;

namespace StockFlow.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllCategoriesAsync();
    Task<int> CreateCategoryAsync(CreateCategoryDto dto);
    
    Task<bool> UpdateCategoryAsync(int id, CreateCategoryDto dto); // Bunu eklemeyi unutma
    Task<bool> DeleteCategoryAsync(int id); // Bunu eklemeyi unutma
}