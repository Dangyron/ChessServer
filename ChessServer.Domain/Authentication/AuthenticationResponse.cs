namespace ChessServer.Domain.Authentication;

public class AuthenticationResponse
{
    public string Username { get; set; } = null!;
    public string Token { get; set; } = null!;
}