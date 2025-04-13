namespace qplix_portfolio_management.Application.Abstractions.Persistence;

public interface IBaseRepository<T> where T : class
{
    // Create
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    // Read
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    // Update
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    // Delete
    Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteByIdAsync(object id, CancellationToken cancellationToken = default);
}