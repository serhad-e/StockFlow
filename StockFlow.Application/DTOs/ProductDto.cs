namespace StockFlow.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
}