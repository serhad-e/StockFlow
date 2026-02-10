namespace StockFlow.Domain.Common;
using System.ComponentModel.DataAnnotations;
public abstract class BaseEntity
{   
    [Key]
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public bool IsDeleted { get; set; } = false; // Soft Delete için (Veriyi silmez, gizler)
}