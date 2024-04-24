using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using ChessServer.Data.Data;
using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ChessServer.Data.Repositories;

public abstract class Repository<T> : IRepository<T> where T : Entity
{
    protected readonly ChessDbContext DbContext;
    protected readonly DbSet<T> Set;

    protected Repository(ChessDbContext dbContext)
    {
        DbContext = dbContext;
        Set = dbContext.Set<T>();
    }

    public Task<IEnumerable<T>?> GetAllAsync(Expression<Func<T, bool>>? filter = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<T>?>(Set.AsNoTracking().Where(filter ?? (f => true)));
    }

    public Task<T?> GetAsync([NotNull] Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        return Set.AsNoTracking().FirstOrDefaultAsync(filter, cancellationToken);
    }

    public Task<T?> GetByIdAsync([NotNull]Guid id, CancellationToken cancellationToken = default)
    {
        return Set.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync([NotNull] T entity, CancellationToken cancellationToken = default)
    {
        await Set.AddAsync(entity, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(DbContext.SaveChangesAsync(cancellationToken));
    }

    public void Remove([NotNull] T entity)
    {
        Set.Remove(entity);
    }
}