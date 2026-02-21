using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Interfaces.IRepositories;
using StockFlow.Infrastructure.Persistence;
using System.Linq.Expressions;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Common;
namespace StockFlow.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, 
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;
        if (filter != null) query = query.Where(filter);
        foreach (var include in includes) query = query.Include(include);
        if (orderBy != null) return await orderBy(query).ToListAsync();
        return await query.ToListAsync();
    }

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    public void Update(T entity) => _dbSet.Update(entity);
    public void Delete(T entity) => _dbSet.Remove(entity);
    public async Task<T?> GetByIdWithItemsAsync(int id)
    {
        // 1. Tipi Order olarak kontrol et ve Include işlemlerini yap
        if (typeof(T) == typeof(Order))
        {
            var result = await _context.Set<Order>()
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);
            
            return result as T;
        }

        // 2. Order değilse standart davranışa dön
        // Burada 'id' üzerinden arama yapmak için Set<T>().FindAsync kullanmak en güvenlisidir
        return await _context.Set<T>().FindAsync(id);
    }
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        // Veritabanı setine gidip verilen kurala uyan kayıt var mı bakar
        return await _dbSet.AnyAsync(predicate);
    }
}