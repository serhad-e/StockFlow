using System.Linq.Expressions;

namespace StockFlow.Application.Interfaces.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    // Filtreleme, Sıralama ve İlişkili Tabloları (Include) çekebilen gelişmiş liste metodu
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, 
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        params Expression<Func<T, object>>[] includes);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}