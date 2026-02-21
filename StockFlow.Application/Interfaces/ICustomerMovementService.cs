using StockFlow.Application.DTOs;
using StockFlow.Domain.Entities; // CustomerMovement listelemek için gerekebilir

namespace StockFlow.Application.Interfaces;

public interface ICustomerMovementService
{
    // Müşteriye borç veya ödeme ekler
    Task<bool> AddMovementAsync(CreateCustomerMovementDto dto);
    
    // Müşterinin hareket dökümünü getirir (Ekstre)
    Task<List<CustomerMovementListDto>> GetCustomerHistoryAsync(int customerId);
}