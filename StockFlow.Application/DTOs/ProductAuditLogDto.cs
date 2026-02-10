namespace StockFlow.Application.DTOs;

public class ProductAuditLogDto
{
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Action { get; set; }
}