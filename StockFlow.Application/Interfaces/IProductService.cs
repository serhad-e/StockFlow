namespace StockFlow.Application.Interfaces;
using StockFlow.Application.DTOs;
public interface IProductService
{
    Task<int> CreateProductAsync(CreateProductDto dto);
    // YENİ EKLEDİĞİMİZ GÜNCELLEME METODU
    Task<bool> UpdateProductAsync(int id, UpdateProductDto dto);

    Task<List<ProductAuditLogDto>> GetProductHistoryAsync(int productId);

    // İleride geçmişi görmek istersen bunu da şimdiden ekleyebiliriz
    Task<bool> UpdateStockAsync(int productId, int quantity); // Stok artır/azalt
    Task<List<ProductDto>> GetLowStockProductsAsync();
    Task<List<ProductListDto>> GetAllProductsAsync(string? search = null, int? categoryId = null);    
    Task<bool> DeleteProductAsync(int id);
}