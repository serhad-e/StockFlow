using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;

namespace StockFlow.API.Controllers;

// Bu öznitelik, bu sınıfın bir API Controller olduğunu ve otomatik JSON dönüştürme yapacağını söyler.
[ApiController] 
// Adres satırında nasıl görüneceğini belirler: https://localhost:xxxx/api/customers
[Route("api/[controller]")] 
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    // Dependency Injection: Sisteme daha önce tanıttığımız servisi burada kapıdan içeri alıyoruz.
    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    // YENİ MÜŞTERİ EKLEME (POST İsteği)
    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerDto dto)
    {
        // Gelen paketi (dto) servise gönderiyoruz
        var id = await _customerService.CreateCustomerAsync(dto);
        
        // İşlem başarılıysa ID ve mesaj dönüyoruz
        return Ok(new { id = id, message = "Müşteri başarıyla oluşturuldu." });
    }

    // TÜM MÜŞTERİLERİ LİSTELEME (GET İsteği)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Servisten listeyi istiyoruz
        var customers = await _customerService.GetAllCustomersAsync();
        
        // Listeyi kullanıcıya gönderiyoruz
        return Ok(customers);
    }
}