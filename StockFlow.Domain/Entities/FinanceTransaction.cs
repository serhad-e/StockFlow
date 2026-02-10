namespace StockFlow.Domain.Entities;
using StockFlow.Domain.Common;
using StockFlow.Domain.Enums;
public class FinanceTransaction:BaseEntity
{
    // İşlem Türü (Gelir mi, Gider mi?)
    public TransactionType Type { get; set; } 
    
    // Ödeme Yöntemi (Nakit, Kredi Kartı vb.)
    public PaymentMethod PaymentMethod { get; set; }

    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    // Kategori: "Kira", "Maaş", "Satış Tahsilatı", "Vergi" vb.
    public string Category { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Opsiyonel İlişki: Eğer bu gelir bir siparişten geliyorsa hangi sipariş?
    public int? OrderId { get; set; }
    public Order? Order { get; set; }
}