using System.ComponentModel.DataAnnotations.Schema;

namespace ChessServer.Domain.Models;

public sealed class Game : Entity
{
    public const string DefaultFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public Guid BlackPlayerId { get; set; }
    public Guid WhitePlayerId { get; set; }
    public string Fen { get; set; } = string.Empty;
    public bool IsRating { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public GameResult Result { get; set; }
}