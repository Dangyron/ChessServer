using ChessLogic.Moves;

namespace ChessServer.Domain.DtoS;

public sealed class MoveResponse
{
    public Guid Id { get; set; }
    public Move Move { get; set; } = null!;
}