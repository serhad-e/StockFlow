namespace StockFlow.Application.DTOs;

public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public string CategoryName { get; set; } = string.Empty; // ID yerine isim döneceğiz
    public DateTime CreatedDate { get; set; }
}