using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;

namespace StockFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _categoryService.GetAllCategoriesAsync());

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        var id = await _categoryService.CreateCategoryAsync(dto);
        return Ok(new { id, message = "Kategori başarıyla oluşturuldu." });
    }

    // YENİ: Kategori Güncelleme (Title Case kuralı serviste işlenecek)
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateCategoryDto dto)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, dto);
        if (!result) return NotFound(new { message = "Kategori bulunamadı." });
        
        return Ok(new { message = "Kategori başarıyla güncellendi." });
    }

    // YENİ: Kategori Silme (Soft Delete)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        if (!result) return NotFound(new { message = "Kategori bulunamadı." });
        
        return Ok(new { message = "Kategori başarıyla silindi." });
    }
}