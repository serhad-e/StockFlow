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
}