namespace StockFlow.Domain.Entities;
using StockFlow.Domain.Common;
public class OrderItem:BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Satış Anındaki Veriler (Snapshot)
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Satış anındaki birim fiyat
    public int TaxRate { get; set; }       // Satış anındaki KDV oranı
    
    // Hesaplanan Alan
    public decimal TotalPrice => Quantity * UnitPrice;
    
    public int ReturnedQuantity { get; set; } = 0;
    
    public int ActiveQuantity => Quantity - ReturnedQuantity;
}