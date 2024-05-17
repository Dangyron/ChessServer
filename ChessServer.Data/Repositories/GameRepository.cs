using ChessServer.Data.Data;
using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ChessServer.Data.Repositories;

public class GameRepository : Repository<Game>, IGameRepository
{
    public GameRepository(ChessDbContext dbContext) : base(dbContext)
    {
    }

    public async Task UpdateAsync(Game entity, CancellationToken? cancellationToken = default)
    {
        var game = await Set.FirstOrDefaultAsync(p => p.Id == entity.Id, cancellationToken ?? default);

        if (game == null)
            return;

        game.EndTime = entity.EndTime;
        game.Fen = entity.Fen;
        game.Pgn = entity.Pgn;
        game.Result = entity.Result;
    }

    public async Task<IEnumerable<Game>?> FindFor(Guid id, CancellationToken? cancellationToken = default)
    {
        return await Set.Where(game => game.BlackPlayerId == id || game.WhitePlayerId == id).ToListAsync();
    }
}