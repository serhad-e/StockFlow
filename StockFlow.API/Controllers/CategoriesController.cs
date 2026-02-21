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
    public async Task<IActionResult> GetAll()
    {
        try 
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception)
        {
            return BadRequest(new { message = "Kategoriler listelenirken bir hata oluştu." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        try 
        {
            // FluentValidation sayesinde dto.Name'in dolu olduğu garanti edildi.
            var id = await _categoryService.CreateCategoryAsync(dto);
            return Ok(new { id, message = "Kategori başarıyla oluşturuldu." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateCategoryDto dto)
    {
        try 
        {
            var result = await _categoryService.UpdateCategoryAsync(id, dto);
            if (!result) return NotFound(new { message = "Kategori bulunamadı veya silinmiş." });
            
            return Ok(new { message = "Kategori başarıyla güncellendi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try 
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result) return NotFound(new { message = "Kategori bulunamadı." });
            
            return Ok(new { message = "Kategori başarıyla silindi (arşivlendi)." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Kategori silinirken bir hata oluştu: " + ex.Message });
        }
    }
}