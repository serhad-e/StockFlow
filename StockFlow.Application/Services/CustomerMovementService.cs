using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Services;

public class CustomerMovementService : ICustomerMovementService
{
    private readonly IUnitOfWork _uow;

    public CustomerMovementService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> AddMovementAsync(CreateCustomerMovementDto dto)
    {
        var customer = await _uow.Customers.GetByIdAsync(dto.CustomerId);
        if (customer == null || customer.IsDeleted) return false;

        var movement = new CustomerMovement
        {
            CustomerId = dto.CustomerId,
            Amount = dto.Amount,
            Type = (CustomerMovementType)dto.Type,
            PaymentMethod = (PaymentMethod)dto.PaymentMethod, // Tür adıyla çakışma giderildi
            IsPaid = dto.IsPaid,
            Description = dto.Description,
            OperationDate = DateTime.UtcNow
        };

        if (movement.Type == CustomerMovementType.Debt) 
        {
            if (!dto.IsPaid) customer.Balance += dto.Amount;
        }
        else if (movement.Type == CustomerMovementType.Credit) 
        {
            customer.Balance -= dto.Amount;
        }

        await _uow.CustomerMovements.AddAsync(movement);
        _uow.Customers.Update(customer);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<List<CustomerMovementListDto>> GetCustomerHistoryAsync(int customerId)
    {
        var history = await _uow.CustomerMovements.GetAllAsync(
            filter: x => x.CustomerId == customerId,
            orderBy: q => q.OrderByDescending(x => x.OperationDate)
        );

        return history.Select(x => new CustomerMovementListDto
        {
            Id = x.Id,
            Amount = x.Amount,
            Type = x.Type.ToString(),
            PaymentMethod = x.PaymentMethod.ToString(),
            IsPaid = x.IsPaid,
            Description = x.Description,
            OperationDate = x.OperationDate
        }).ToList();
    }
}