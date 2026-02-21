namespace StockFlow.Application.DTOs;

// Yeni Sipariş oluştururken dışarıdan alacağımız veriler
public class CreateOrderDto
{
    public int CustomerId { get; set; }
    public string? Note { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

// Siparişin içindeki her bir satır için gereken veriler
public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

// Siparişleri listelerken kullanabileceğin genel yapı
public class OrderListDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}