using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _uow;

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
}