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
        try 
        {
            // FluentValidation sayesinde dto içindeki Name, Price gibi alanlar dolu ve geçerli geliyor.
            var id = await _productService.CreateProductAsync(dto);
            return Ok(new { id = id, message = "Ürün başarıyla oluşturuldu." });
        }
        catch (Exception ex)
        {
            // Servis katmanında oluşabilecek öngörülemeyen hataları yakalar
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        try 
        {
            var result = await _productService.UpdateProductAsync(id, dto);

            if (!result)
            {
                return NotFound(new { message = $"{id} numaralı ürün bulunamadı." });
            }

            return Ok(new { message = "Ürün bilgileri ve fiyatlandırması başarıyla güncellendi." });
        }
        catch (Exception ex)
        {
            // ProductService.cs içinde yaptığımız matematiksel hesaplama hatalarını yakalar
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        // Geçmiş (Audit Log) kayıtlarını getirir
        var history = await _productService.GetProductHistoryAsync(id);
        return Ok(history);
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        // Kritik stok seviyesindeki ürünleri getirir
        var products = await _productService.GetLowStockProductsAsync();
        return Ok(products);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? categoryId)
    {
        var products = await _productService.GetAllProductsAsync(search, categoryId);
        return Ok(products);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try 
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result) return NotFound(new { message = "Ürün bulunamadı." });

            return Ok(new { message = "Ürün başarıyla silindi (arşivlendi)." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}