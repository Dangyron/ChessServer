namespace ChessServer.Domain.DtoS;

public class GameMoveRequest
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public string Fen { get; set; } = null!;
    public string Pgn { get; set; } = null!;
}