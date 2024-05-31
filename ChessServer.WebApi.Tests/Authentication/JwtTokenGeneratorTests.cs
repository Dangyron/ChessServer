using System.IdentityModel.Tokens.Jwt;
using ChessServer.Domain.Dtos;
using ChessServer.WebApi.Authentication;
using ChessServer.WebApi.Authentication.Common;
using ChessServer.WebApi.Common.Extensions;
using Microsoft.Extensions.Options;

namespace ChessServer.WebApi.Tests.Authentication;

public sealed class JwtTokenGeneratorTests
{
    [Fact]
    public void GetInfo_ValidRequest_ReturnsString()
    {
        // Arrange
        var jwtTokenSettings = new JwtTokenSettings
        {
            SecretKey = "mega-super-secret-key-which-no-one-knows",
            Issuer = "hzz",
            Audience = "hzz",
            ExpiresInDays = 1,
        };
        var userJwtDto = new UserJwtDto { Id = Guid.NewGuid(), Username = "username", };

        var controller = new JwtTokenGenerator(Options.Create(jwtTokenSettings));
        
        // Act
        var result = controller.Generate(userJwtDto);
        
        // Assert
        var claims = new JwtSecurityTokenHandler().ReadToken(result) as JwtSecurityToken;
        
        Assert.Equal(userJwtDto.Id, claims!.GetId());
        Assert.Equal(userJwtDto.Username, claims!.Claims.First(cl => cl.Type == JwtRegisteredClaimNames.Name).Value);
    }
}