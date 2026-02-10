using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;

namespace StockFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        var id = await _productService.CreateProductAsync(dto);
        return Ok(new { id = id, message = "Ürün başarıyla oluşturuldu." });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }
    
    [HttpPut("{id}")] // Buradaki {id} ifadesi URL'den gelen 2 rakamını yakalar
    public async Task<IActionResult> Update(int id, UpdateProductDto dto)
    {
        var result = await _productService.UpdateProductAsync(id, dto);
        if (!result) return NotFound(new { message = "Ürün bulunamadı." });

        return Ok(new { message = "Ürün başarıyla güncellendi." });
    }
    
    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var history = await _productService.GetProductHistoryAsync(id);
        return Ok(history);
    }
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var products = await _productService.GetLowStockProductsAsync();
        return Ok(products);
    }
}