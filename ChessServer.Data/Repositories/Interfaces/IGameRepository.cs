using ChessServer.Domain.Models;

namespace ChessServer.Data.Repositories.Interfaces;

public interface IGameRepository : IRepository<Game>
{
    Task UpdateAsync(Game entity, CancellationToken? cancellationToken = default);
    Task<IEnumerable<Game>?> FindFor(Guid id, CancellationToken? cancellationToken = default);
}