using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ChessServer.WebApi.Authentication;

namespace ChessServer.WebApi.Common.Extensions;

public static class UserExtensions
{
    public static Guid GetId(this ClaimsPrincipal claims) =>
        Guid.Parse(claims.Claims.First(claim => claim.Type == CustomClaims.UserId).Value);
    public static Guid GetId(this JwtSecurityToken claims) =>
        Guid.Parse(claims.Claims.First(claim => claim.Type == CustomClaims.UserId).Value);
}