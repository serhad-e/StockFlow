namespace StockFlow.Application.DTOs;

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; } // Boş bırakılabilir (Opsiyonel)
    public decimal PurchasePrice { get; set; }
    public decimal? ProfitRate { get; set; }
    public decimal? SalePrice { get; set; }
    public int? TaxRate { get; set; } // Özel KDV oranı girilebilir
    public int InitialStock { get; set; }
    
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; } // Kullanıcı ID yerine ismi görsün diye
}