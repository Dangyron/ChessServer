using ChessServer.Domain.Models;

namespace ChessServer.Domain.DtoS;

public sealed class AddUserInfoDto
{
    public int? Age { get; set; }
    public string? Country { get; set; }
    public Gender? Gender { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}