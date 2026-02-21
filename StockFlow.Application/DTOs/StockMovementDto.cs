namespace StockFlow.Application.DTOs;

public class CreateStockMovementDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int Type { get; set; } // 1: Giriş (In), 2: Çıkış (Out)
    public string Reason { get; set; } = string.Empty;
}

public class StockMovementListDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime MovementDate { get; set; }
}