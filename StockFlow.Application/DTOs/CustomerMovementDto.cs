namespace StockFlow.Application.DTOs;

public class CreateCustomerMovementDto
{
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public int Type { get; set; }           
    public int PaymentMethod { get; set; }   
    public bool IsPaid { get; set; }         
    public string Description { get; set; } = string.Empty;
}

public class CustomerMovementListDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime OperationDate { get; set; }
    // HATA BURADAYDI: Bu iki alanı ekliyoruz
    public bool IsPaid { get; set; }
    public string Description { get; set; } = string.Empty;
}