using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;

namespace StockFlow.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _uow;

    public DashboardService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        // Tüm aktif verileri Unit of Work üzerinden çekiyoruz
        var products = await _uow.Products.GetAllAsync(p => !p.IsDeleted);
        var categories = await _uow.Categories.GetAllAsync(c => !c.IsDeleted);
        var customers = await _uow.Customers.GetAllAsync(cust => !cust.IsDeleted);
        
        // Son hareketleri ürün isimleriyle beraber çekelim
        var movements = await _uow.StockMovements.GetAllAsync(
            orderBy: q => q.OrderByDescending(x => x.MovementDate),
            includes: x => x.Product
        );

        var summary = new DashboardSummaryDto
        {
            TotalProducts = products.Count,
            TotalStockQuantity = products.Sum(p => p.StockQuantity),
            // Finansal özet: (Stok Adedi * Satış Fiyatı)
            TotalInventoryValue = products.Sum(p => p.StockQuantity * p.SalePrice),
            CriticalStockCount = products.Count(p => p.StockQuantity <= p.CriticalStockLevel),
            TotalCategories = categories.Count,
            TotalCustomers = customers.Count,
            
            // Son 5 hareketi anlamlı cümlelere dönüştürelim
            RecentActivities = movements.Take(5).Select(m => new RecentActivityDto
            {
                Message = $"{m.Product.Name} ürününe {m.Quantity} adet {(m.Type.ToString() == "In" ? "giriş" : "çıkış")} yapıldı. (Neden: {m.Reason})",
                Date = m.MovementDate
            }).ToList()
        };

        return summary;
    }
}