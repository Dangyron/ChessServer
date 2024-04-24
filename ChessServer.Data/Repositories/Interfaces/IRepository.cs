using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using ChessServer.Domain.Models;

namespace ChessServer.Data.Repositories.Interfaces;

public interface IRepository<T> where T : Entity
{
    Task<IEnumerable<T>?> GetAllAsync(Expression<Func<T, bool>>? filter = null, CancellationToken cancellationToken = default);
    Task<T?> GetAsync([NotNull] Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync([NotNull] Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    void Remove(T entity);
}