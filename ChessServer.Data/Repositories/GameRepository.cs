using ChessServer.Data.Data;
using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.Models;

namespace ChessServer.Data.Repositories;

public class GameRepository : Repository<Game>, IGameRepository
{
    public GameRepository(ChessDbContext dbContext) : base(dbContext)
    {
    }
    
    public Task Update(Game game, CancellationToken? cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}