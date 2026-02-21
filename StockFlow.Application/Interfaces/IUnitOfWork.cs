using StockFlow.Application.Interfaces.IRepositories;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Product> Products { get; }
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<Customer> Customers { get; }
    IGenericRepository<ProductAuditLog> AuditLogs { get; }
    IGenericRepository<StockMovement> StockMovements { get; }
    Task<int> SaveChangesAsync();
    IGenericRepository<CustomerMovement> CustomerMovements { get; }
    IGenericRepository<Order> Orders { get; }
    IGenericRepository<OrderItem> OrderItems { get; }
    
}