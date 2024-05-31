namespace ChessServer.Domain.Models;

public sealed class Game : Entity
{
    public Guid BlackPlayerId { get; set; }
    public Guid WhitePlayerId { get; set; }
    public string Fen { get; set; } = string.Empty;
    public string? Pgn { get; set; }
    public bool IsRating { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public GameResult Result { get; set; }
}