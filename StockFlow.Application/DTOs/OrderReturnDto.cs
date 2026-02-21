namespace StockFlow.Application.DTOs;

public class CreateOrderReturnDto
{
    public int OrderId { get; set; } // Hangi faturadan iade?
    public List<CreateOrderReturnItemDto> Items { get; set; } = new();
}

public class CreateOrderReturnItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}