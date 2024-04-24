namespace ChessServer.Domain.Models;

public class BoardPosition
{
    public int Number { get; set; }
    public string Fen { get; set; } = null!;
    public string Move { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}