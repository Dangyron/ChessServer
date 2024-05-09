using ChessServer.Domain.Models;

namespace ChessServer.Data.Repositories.Interfaces;

public interface IGameRepository : IRepository<Game>
{
    Task UpdateAsync(Game entity, CancellationToken? cancellationToken = default);
    IAsyncEnumerable<Game>? FindFor(Guid id, CancellationToken? cancellationToken = default);
}