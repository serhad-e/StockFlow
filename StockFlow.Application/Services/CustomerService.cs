using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _uow; // Değişken ismin _uow olarak tanımlı

    public CustomerService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<int> CreateCustomerAsync(CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            Name = dto.Name,
            CompanyName = dto.CompanyName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            Balance = 0 
        };

        await _uow.Customers.AddAsync(customer);
        await _uow.SaveChangesAsync();
        return customer.Id;
    }

    public async Task<List<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await _uow.Customers.GetAllAsync(x => !x.IsDeleted);
        return customers.Select(x => new CustomerDto
        {
            Id = x.Id,
            Name = x.Name,
            CompanyName = x.CompanyName,
            Phone = x.Phone,
            Balance = x.Balance
        }).ToList();
    }

    public async Task<decimal> GetCustomerBalanceAsync(int customerId)
    {
        var customer = await _uow.Customers.GetByIdAsync(customerId);
        return customer?.Balance ?? 0;
    }

    // Hatalı kısımları _uow olarak düzelttik:
    public async Task<bool> AnyEmailAsync(string email)
    {
        // _unitOfWork yerine _uow kullanıldı
        return await _uow.Customers.AnyAsync(x => x.Email == email && !x.IsDeleted);
    }

    public async Task<bool> AnyPhoneAsync(string phone)
    {
        // _unitOfWork yerine _uow kullanıldı
        return await _uow.Customers.AnyAsync(x => x.Phone == phone && !x.IsDeleted);
    }
}