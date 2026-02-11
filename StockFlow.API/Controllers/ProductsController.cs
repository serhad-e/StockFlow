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

    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        // Servis katmanındaki güncelleme mantığını çağırıyoruz
        var result = await _productService.UpdateProductAsync(id, dto);

        if (!result)
        {
            return NotFound(new { message = $"{id} numaralı ürün bulunamadı." });
        }

        return Ok(new { message = "Ürün bilgileri ve fiyatlandırması başarıyla güncellendi." });
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
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? categoryId)
    {
        // Interface üzerinden metodumuzu çağırıyoruz
        var products = await _productService.GetAllProductsAsync(search, categoryId);
    
        // Liste boş olsa bile 200 OK döner ama boş liste [] döner
        return Ok(products);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productService.DeleteProductAsync(id);
        if (!result) return NotFound(new { message = "Ürün bulunamadı." });

        return Ok(new { message = "Ürün başarıyla silindi (arşivlendi)." });
    }
}