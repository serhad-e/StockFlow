using StockFlow.Application.Interfaces;
using StockFlow.Application.Interfaces.IRepositories;
using StockFlow.Domain.Entities;
using StockFlow.Infrastructure.Persistence.Repositories;

namespace StockFlow.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Products = new GenericRepository<Product>(_context);
        Categories = new GenericRepository<Category>(_context);
        Customers = new GenericRepository<Customer>(_context);
        AuditLogs = new GenericRepository<ProductAuditLog>(_context);
        StockMovements = new GenericRepository<StockMovement>(_context);
    }

    public IGenericRepository<Product> Products { get; }
    public IGenericRepository<Category> Categories { get; }
    public IGenericRepository<Customer> Customers { get; }
    public IGenericRepository<ProductAuditLog> AuditLogs { get; }
    public IGenericRepository<StockMovement> StockMovements { get; }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    public void Dispose() => _context.Dispose();
}