using StockFlow.Domain.Common;

namespace StockFlow.Domain.Entities;

public class ProductAuditLog : BaseEntity
{
    public int ProductId { get; set; }
    public string FieldName { get; set; } = string.Empty; // Hangi alan değişti? (Fiyat, İsim vb.)
    public string? OldValue { get; set; } // Eski değer
    public string? NewValue { get; set; } // Yeni değer
    public string? Action { get; set; }    // Güncelleme mi, Oluşturma mı?
}