using StockFlow.Application.DTOs;

namespace StockFlow.Application.Interfaces;

public interface IOrderService
{
    // Tek bir işlemde hem stok düşer, hem cari günceller, hem sipariş oluşturur.
    Task<bool> CreateOrderAsync(CreateOrderDto dto);
    
    Task<bool> CreateReturnAsync(CreateOrderReturnDto dto);
}