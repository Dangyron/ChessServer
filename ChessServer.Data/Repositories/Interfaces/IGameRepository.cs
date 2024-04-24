using ChessServer.Domain.Models;

namespace ChessServer.Data.Repositories.Interfaces;

public interface IGameRepository : IRepository<Game>
{
    Task Update(Game game, CancellationToken? cancellationToken = default);
}