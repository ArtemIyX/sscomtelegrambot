using Microsoft.EntityFrameworkCore.Storage;

namespace SS.Notifier.Data.Repository;

public interface IRepository<T, TId> where T : class
{
    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    T Update(T entity);
    Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default);
    Task<T?> FindByKeyAsync(TId id, CancellationToken cancellationToken = default);
    IQueryable<T> AsQueryable();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

}