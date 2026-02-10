namespace StockFlow.Application.Services;
using Microsoft.EntityFrameworkCore;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;
using StockFlow.Infrastructure.Persistence;

public class CustomerService:ICustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
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
            Balance = 0 // Yeni müşteri sıfır bakiye ile başlar
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        
        return customer.Id;
    }

    public async Task<List<CustomerDto>> GetAllCustomersAsync()
    {
        return await _context.Customers
            .Where(x => !x.IsDeleted) // Sadece silinmemişleri getir
            .Select(x => new CustomerDto
            {
                Id = x.Id,
                Name = x.Name,
                CompanyName = x.CompanyName,
                Phone = x.Phone,
                Balance = x.Balance
            }).ToListAsync();
    }

    public Task<decimal> GetCustomerBalanceAsync(int customerId)
    {
        return _context.Customers
            .Where(x => x.Id == customerId)
            .Select(x => x.Balance)
            .FirstOrDefaultAsync();
    }
}