using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChessServer.Domain.DtoS;
using ChessServer.WebApi.Authentication.Common;
using ChessServer.WebApi.Authentication.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChessServer.WebApi.Authentication;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtTokenSettings _jwtTokenSettings;

    public JwtTokenGenerator(IOptions<JwtTokenSettings> jwtTokenSettings)
    {
        _jwtTokenSettings = jwtTokenSettings.Value;
    }

    public string Generate(UserJwtDto userDto)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtTokenSettings.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userDto.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, userDto.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var secureToken = new JwtSecurityToken(
            issuer: _jwtTokenSettings.Issuer,
            audience: _jwtTokenSettings.Audience,
            expires: DateTime.UtcNow.AddDays(_jwtTokenSettings.ExpiresInDays),
            signingCredentials: signingCredentials,
            claims: claims
        );

        return new JwtSecurityTokenHandler().WriteToken(secureToken);
    }
}