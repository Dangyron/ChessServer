namespace ChessServer.Domain.DtoS;

public class GameDto
{
    public Guid Id { get; set; }
    public string Move { get; set; } = null!;
}