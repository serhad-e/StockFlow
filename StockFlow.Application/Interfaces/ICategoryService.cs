using StockFlow.Application.DTOs;

namespace StockFlow.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllCategoriesAsync();
    Task<int> CreateCategoryAsync(CreateCategoryDto dto);
}