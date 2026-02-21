namespace StockFlow.Application.DTOs;

public class DashboardSummaryDto
{
    public int TotalProducts { get; set; }           // Kaç çeşit ürün var?
    public int TotalStockQuantity { get; set; }      // Depodaki toplam ürün adedi nedir?
    public decimal TotalInventoryValue { get; set; } // Depodaki malların toplam satış değeri (TL)
    public int CriticalStockCount { get; set; }      // Stoğu bitmek üzere olan kaç ürün var?
    public int TotalCategories { get; set; }         // Toplam kategori sayısı
    public int TotalCustomers { get; set; }          // Kayıtlı müşteri sayısı
    public List<RecentActivityDto> RecentActivities { get; set; } = new(); // Son 5 işlem
}

public class RecentActivityDto
{
    public string Message { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}