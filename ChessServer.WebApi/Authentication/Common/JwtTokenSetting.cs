namespace ChessServer.WebApi.Authentication.Common;

public sealed class JwtTokenSettings
{
    public const string SectionName = nameof(JwtTokenSettings);
    public string SecretKey { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpiresInDays { get; set; }
}