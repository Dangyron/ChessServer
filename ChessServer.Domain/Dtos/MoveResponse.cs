namespace ChessServer.Domain.Dtos;

public sealed class MoveResponse
{
    public Guid Id { get; set; }
    public string Move { get; set; } = null!;
}