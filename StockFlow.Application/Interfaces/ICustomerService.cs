namespace StockFlow.Application.Interfaces;
using StockFlow.Application.DTOs;
public interface ICustomerService
{
    // Yeni bir müşteri oluşturur
    Task<int> CreateCustomerAsync(CreateCustomerDto dto);
    
    // Tüm müşterileri listeler
    Task<List<CustomerDto>> GetAllCustomersAsync();
    
    // Tek bir müşterinin borç/alacak bilgisini getirir
    Task<decimal> GetCustomerBalanceAsync(int customerId);
}