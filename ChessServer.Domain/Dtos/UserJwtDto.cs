namespace ChessServer.Domain.Dtos;

public sealed class UserJwtDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
}