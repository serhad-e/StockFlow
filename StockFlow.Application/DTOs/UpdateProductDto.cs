namespace StockFlow.Application.DTOs;

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? ProfitRate { get; set; }
    public int CriticalStockLevel { get; set; }
}