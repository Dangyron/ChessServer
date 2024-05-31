namespace ChessServer.Domain.Dtos;

public sealed class MoveRequest
{
    public Guid Id { get; set; }
    public string Move { get; set; } = null!;
}