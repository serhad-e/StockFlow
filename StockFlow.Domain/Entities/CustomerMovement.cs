using StockFlow.Domain.Common;

namespace StockFlow.Domain.Entities;

public enum CustomerMovementType 
{ 
    Debt = 1,    // Satış (Borçlandırır)
    Credit = 2,  // Ödeme (Alacaklandırır)
    Return = 3   // İADE (Ürün geri geldiği için borcu azaltır)
}
public enum PaymentMethod { Cash = 1, CreditCard = 2, BankTransfer = 3 } // Bunu eklediğinden emin ol

public class CustomerMovement : BaseEntity
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public decimal Amount { get; set; }
    public CustomerMovementType Type { get; set; }
    
    // HATA BURADAYDI: Bu iki satırı ekliyoruz
    public PaymentMethod PaymentMethod { get; set; } 
    public bool IsPaid { get; set; } 
    
    public string Description { get; set; } = string.Empty;
    public DateTime OperationDate { get; set; } = DateTime.UtcNow;
}