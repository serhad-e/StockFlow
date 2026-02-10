namespace StockFlow.Application.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string Phone { get; set; } = string.Empty;
    public decimal Balance { get; set; } // En kritik bilgi
}