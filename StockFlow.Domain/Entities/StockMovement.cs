namespace StockFlow.Domain.Entities;
using StockFlow.Domain.Common;

public enum MovementType 
{ 
    In = 1,      // Alış
    Out = 2,     // Satış
    Return = 3   // İADE (Depoya ürün girişi)
}

public class StockMovement : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; } 
    public MovementType Type { get; set; } // Giriş mi (+) yoksa Çıkış mı (-) ?
    public string Reason { get; set; } = string.Empty; 
    
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
}