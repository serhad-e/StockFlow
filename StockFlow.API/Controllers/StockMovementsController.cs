using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;

namespace StockFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockMovementsController : ControllerBase
{
    private readonly IStockMovementService _movementService;

    public StockMovementsController(IStockMovementService movementService)
    {
        _movementService = movementService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMovement([FromBody] CreateStockMovementDto dto)
    {
        try 
        {
            // FluentValidation sayesinde miktar > 0 kontrolü kapıda yapıldı.
            var result = await _movementService.CreateMovementAsync(dto);
            
            if (!result)
            {
                // Burası genellikle "Ürün bulunamadı" veya "Stok yetersiz" durumlarında döner.
                return BadRequest(new { message = "Stok hareketi oluşturulamadı. Ürün bulunamamış veya stok yetersiz olabilir." });
            }

            return Ok(new { message = "Stok hareketi başarıyla işlendi ve ana stok güncellendi." });
        }
        catch (Exception ex)
        {
            // Eğer servis içinde 'throw new Exception' kullandıysan burası yakalar.
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetProductMovements(int productId)
    {
        // Ürüne ait tüm hareket geçmişini listeler.
        var movements = await _movementService.GetProductMovementsAsync(productId);
        return Ok(movements);
    }
}