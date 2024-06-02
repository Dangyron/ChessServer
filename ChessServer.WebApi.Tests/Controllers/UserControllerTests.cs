using System.Collections.Concurrent;
using System.Security.Claims;
using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.Models;
using ChessServer.WebApi.Authentication;
using ChessServer.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace ChessServer.WebApi.Tests.Controllers;

public sealed class UserControllerTests
{
    [Fact]
    public async Task GetInfo_ValidRequest_ReturnsString()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var gameRepository = Substitute.For<IGameRepository>();
        var cancellationTokenSource = new CancellationTokenSource();
        var httpContext = Substitute.For<HttpContext>();
        var userId = Guid.NewGuid();
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(CustomClaims.UserId, userId.ToString())
        }));

        httpContext.User.Returns(claimsPrincipal);

        var controller = new UserController(userRepository, cancellationTokenSource, gameRepository, new ConcurrentDictionary<Guid, PlayingGame>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
        };

        userRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationTokenSource.Token).Returns(new User{ Id = userId });
        // Act
        var result = await controller.GetInfo();
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var user = Assert.IsType<User>(okResult.Value);
        Assert.Equal(userId, user.Id);
    }
    
    [Fact]
    public async Task Delete_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var gameRepository = Substitute.For<IGameRepository>();
        var cancellationTokenSource = new CancellationTokenSource();
        var httpContext = Substitute.For<HttpContext>();
        var userId = Guid.NewGuid();
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(CustomClaims.UserId, userId.ToString())
        }));

        httpContext.User.Returns(claimsPrincipal);

        var controller = new UserController(userRepository, cancellationTokenSource, gameRepository, new ConcurrentDictionary<Guid, PlayingGame>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
        };

        userRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationTokenSource.Token).Returns(new User{ Id = userId });
        userRepository.Remove(Arg.Any<User>());
        // Act
        var result = await controller.Delete();
        
        // Assert
        Assert.IsType<OkResult>(result);
    }
    
    [Fact]
    public async Task GetGames_ValidRequest_ReturnsOkObjectResult()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var gameRepository = Substitute.For<IGameRepository>();
        var cancellationTokenSource = new CancellationTokenSource();
        var httpContext = Substitute.For<HttpContext>();
        var userId = Guid.NewGuid();
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(CustomClaims.UserId, userId.ToString())
        }));

        httpContext.User.Returns(claimsPrincipal);

        var controller = new UserController(userRepository, cancellationTokenSource, gameRepository, new ConcurrentDictionary<Guid, PlayingGame>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
        };

        gameRepository.FindFor(Arg.Any<Guid>(), cancellationTokenSource.Token).Returns(new List<Game>());
        // Act
        var result = await controller.GetGames();
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var games = Assert.IsType<List<Game>>(okResult.Value);
        Assert.Equal(new List<Game>(), games);
    }
}