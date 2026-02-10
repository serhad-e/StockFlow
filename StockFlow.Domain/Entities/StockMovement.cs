namespace StockFlow.Domain.Entities;
using StockFlow.Domain.Common;
public class StockMovement:BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!; // Navigation Property

    public int Quantity { get; set; } // Eklenen (+) veya Çıkarılan (-) miktar
    public string Reason { get; set; } = string.Empty; // Örn: "Satış", "İade", "Zayi"
    
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
}