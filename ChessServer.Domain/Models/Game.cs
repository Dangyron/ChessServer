using System.ComponentModel.DataAnnotations.Schema;

namespace ChessServer.Domain.Models;

public sealed class Game : Entity
{
    public Guid BlackPlayerId { get; set; }
    public Guid WhitePlayerId { get; set; }
    public string Fen { get; init; } = string.Empty;
    public bool IsRating { get; set; }
    [NotMapped]public List<BoardPosition> Moves { get; set; } = new();
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public GameResult Result { get; set; }
}