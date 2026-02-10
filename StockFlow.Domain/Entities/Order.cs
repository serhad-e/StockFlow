namespace StockFlow.Domain.Entities;
using StockFlow.Domain.Common;
public class Order:BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty; // Örn: INV-2026-001
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    // İlişkiler
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    
    // Toplamlar (Hızlı raporlama için)
    public decimal TotalAmount { get; set; } // KDV Dahil Toplam
    public decimal TotalTax { get; set; }   // Toplam KDV tutarı
    
    // Esneklik: Sipariş kalemleri (Bir siparişin birden fazla ürünü olur)
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    
    public string? Note { get; set; } // Müşteri özel notu
}