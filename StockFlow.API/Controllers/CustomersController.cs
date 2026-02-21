using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;

namespace StockFlow.API.Controllers;

[ApiController] 
[Route("api/[controller]")] 
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerDto dto)
    {
        try 
        {
            // FluentValidation sayesinde dto'nun doluluğu garanti edildi.
            var id = await _customerService.CreateCustomerAsync(dto);
            
            return Ok(new { id = id, message = "Müşteri başarıyla oluşturuldu." });
        }
        catch (Exception ex)
        {
            // Veritabanı çakışmaları veya servis hatalarını yakalar
            return BadRequest(new { message = "Müşteri kaydı sırasında bir hata oluştu: " + ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try 
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }
        catch (Exception )
        {
            return BadRequest(new { message = "Müşteri listesi çekilirken bir hata oluştu." });
        }
    }
}