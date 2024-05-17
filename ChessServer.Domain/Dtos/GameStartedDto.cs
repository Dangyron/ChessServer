using ChessLogic.Pieces;
using ChessServer.Domain.Models;

namespace ChessServer.Domain.DtoS;

public sealed class GameStartedDto
{
    public Guid GameId { get; set; }
    public PlayerColor PlayerColor { get; set; }
}

public sealed class GameEndedDto
{
    public Guid GameId { get; set; }
    public GameResult Result { get; set; }
}