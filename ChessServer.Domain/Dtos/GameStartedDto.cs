using ChessLogic.Pieces;
using ChessServer.Domain.Models;

namespace ChessServer.Domain.Dtos;

public sealed class GameStartedDto
{
    public Guid GameId { get; set; }
    public PlayerColor PlayerColor { get; set; }
    public string OpponentUsername { get; set; } = null!;
}

public sealed class GameEndedDto
{
    public Guid GameId { get; set; }
    public GameResult Result { get; set; }
}