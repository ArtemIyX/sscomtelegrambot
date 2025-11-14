using Microsoft.EntityFrameworkCore.Storage;
using SS.Notifier.Data.Entity;

namespace SS.Notifier.Data.Repository;

public interface IRepository<T, TId> where T : class
{
    public Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    public Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    public T Update(T entity);
    public Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default);
    public IQueryable<T> AsQueryable();
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    public void DeleteRange(List<T> entitiesToDelete);
}