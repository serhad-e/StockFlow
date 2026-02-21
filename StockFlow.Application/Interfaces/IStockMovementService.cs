using StockFlow.Application.DTOs;

namespace StockFlow.Application.Interfaces;

public interface IStockMovementService
{
    // Stok hareketi kaydeder ve ürünün asıl stoğunu günceller
    Task<bool> CreateMovementAsync(CreateStockMovementDto dto);
    
    // Bir ürünün tüm hareket geçmişini getirir
    Task<List<StockMovementListDto>> GetProductMovementsAsync(int productId);
}