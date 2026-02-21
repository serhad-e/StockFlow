using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;

namespace StockFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        try 
        {
            // FluentValidation sayesinde dto doluluğu burada garanti altına alındı.
            var result = await _orderService.CreateOrderAsync(dto);
            
            if (!result)
                return BadRequest(new { message = "Sipariş oluşturulamadı. Lütfen bilgileri kontrol edin." });

            return Ok(new { message = "Sipariş başarıyla oluşturuldu. Stoklar ve cari bakiyeler güncellendi." });
        }
        catch (Exception ex)
        {
            // OrderService içindeki "X ürünü için stok yetersiz!" hatasını yakalar.
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("return")]
    public async Task<IActionResult> CreateReturn([FromBody] CreateOrderReturnDto dto)
    {
        try 
        {
            // Manuel 'if (dto == null)' kontrolü kaldırıldı, Validator bunu hallediyor.
            var result = await _orderService.CreateReturnAsync(dto);

            if (!result)
                return BadRequest(new { message = "İade işlemi gerçekleştirilemedi." });

            return Ok(new { message = "İade işlemi başarıyla tamamlandı. Stok ve müşteri bakiyesi güncellendi." });
        }
        catch (Exception ex)
        {
            // "İade miktarı satış miktarından fazla olamaz" gibi servis hatalarını yakalar.
            return BadRequest(new { message = ex.Message });
        }
    }
}