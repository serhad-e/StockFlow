namespace StockFlow.Domain.Entities;
using StockFlow.Domain.Common;
public class Product:BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Fiyatlandırma
    public decimal PurchasePrice { get; set; } // Alış Fiyatı (Kâr hesabı için)
    public decimal SalePrice { get; set; }     // Satış Fiyatı
    public int TaxRate { get; set; } = 20;     // KDV Oranı (Varsayılan %20)

    // Stok Yönetimi
    public int StockQuantity { get; set; }     // Mevcut Stok
    public int CriticalStockLevel { get; set; } = 5; // Bu sayının altına düşerse uyarı vereceğiz
    public decimal ProfitRate { get; set; } // Veritabanında kâr yüzdesini tutacak
    // İlişki (İleride kategorilere ayırmak istersen esnek bıraktık)
    public int CategoryId { get; set; } // Foreign Key
    public Category? Category { get; set; } // Navigation Property
}