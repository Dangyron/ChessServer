namespace ChessServer.Domain.DtoS;

public sealed class MoveResponse
{
    public Guid Id { get; set; }
    public string Move { get; set; } = null!;
}