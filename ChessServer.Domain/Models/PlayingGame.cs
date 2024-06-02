using ChessLogic;

namespace ChessServer.Domain.Models;

public sealed class PlayingGame
{
    public static readonly PlayingGame None = new(null, Guid.Empty, Guid.Empty);
    
    public PlayingGame(GameState? game, Guid whitePlayer, Guid blackPlayer)
    {
        Game = game;
        WhitePlayer = whitePlayer;
        BlackPlayer = blackPlayer;
    }
    public GameState? Game { get; }
    public Guid WhitePlayer { get; }
    public Guid BlackPlayer { get; }
}