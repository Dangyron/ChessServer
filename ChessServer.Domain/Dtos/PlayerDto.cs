namespace ChessServer.Domain.DtoS;

public sealed class PlayerDto
{
    public string UserName { get; set; } = null!;
    public string Token { get; set; } = null!;
}