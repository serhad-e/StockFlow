namespace StockFlow.Domain.Entities;
using StockFlow.Domain.Common;
public class Customer:BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? CompanyName { get; set; } // Şahıs değil firmaysa
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public decimal Balance { get; set; } = 0; // Müşterinin borç/alacak bakiyesi
}