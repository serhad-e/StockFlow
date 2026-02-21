using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;

namespace StockFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerMovementsController : ControllerBase
{
    private readonly ICustomerMovementService _movementService;

    public CustomerMovementsController(ICustomerMovementService movementService)
    {
        _movementService = movementService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMovement([FromBody] CreateCustomerMovementDto dto)
    {
        var result = await _movementService.AddMovementAsync(dto);
        if (!result)
        {
            return BadRequest(new { message = "İşlem başarısız. Müşteri bulunamadı veya veriler hatalı." });
        }

        return Ok(new { message = "Müşteri hareketi başarıyla kaydedildi ve bakiye güncellendi." });
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomerHistory(int customerId)
    {
        var history = await _movementService.GetCustomerHistoryAsync(customerId);
        return Ok(history);
    }
}