using ChessServer.Domain.Models;

namespace ChessServer.Data.Repositories.Interfaces;

public interface IGameRepository : IRepository<Game>
{
    Task UpdateAsync(Game game, CancellationToken? cancellationToken = default);
}