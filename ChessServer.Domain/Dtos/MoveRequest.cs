using ChessLogic.Moves;

namespace ChessServer.Domain.DtoS;

public class MoveRequest
{
    public Guid Id { get; set; }
    public Move Move { get; set; } = null!;
}