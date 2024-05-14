using System.Collections.Concurrent;
using System.Security.Claims;
using ChessLogic;
using ChessServer.Data.Repositories.Interfaces;
using ChessServer.WebApi.Authentication;
using ChessServer.WebApi.Common;
using ChessServer.WebApi.Common.Interfaces;
using ChessServer.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace ChessServer.WebApi.Tests.Controllers;

public sealed class GameControllerTests
{
    [Fact]
    public async Task AddToPool_WhenPlayerNotInPool_AddsPlayerToPool()
    {
        // Arrange
        var gameRepository = Substitute.For<IGameRepository>();
        var cancellationTokenSource = new CancellationTokenSource();
        var playersPool = new ConcurrentBag<Guid>();
        var playerConnections = new ConcurrentDictionary<Guid, string>();
        var hubContext = Substitute.For<IHubContext<NotificationHub, INotificationHub>>();
        var httpContext = Substitute.For<HttpContext>();
        var userId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(CustomClaims.UserId, userId.ToString())
        }));

        httpContext.User.Returns(user);

        var controller = new GameController(
            gameRepository,
            cancellationTokenSource,
            Options.Create(new ConcurrentDictionary<Guid, GameState>()),
            hubContext,
            Options.Create(playersPool),
            Options.Create(playerConnections)
        )
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
        };

        // Act
        var result = await controller.AddToPool();

        // Assert
        Assert.IsType<OkResult>(result);
        Assert.Contains(userId, playersPool);
    }
    
    [Fact]
    public async Task StartNew_WithValidPlayers_CreatesNewGameAndReturnsOkResult()
    {
        // Arrange
        var gameRepository = Substitute.For<IGameRepository>();
        var cancellationTokenSource = new CancellationTokenSource();
        var playersPool = new ConcurrentBag<Guid>();
        var playerConnections = new ConcurrentDictionary<Guid, string>();
        var groupClients = Substitute.For<IGroupManager>();
        var hubContext = Substitute.For<IHubContext<NotificationHub, INotificationHub>>();
        var controller = new GameController(
            gameRepository,
            cancellationTokenSource,
            Options.Create(new ConcurrentDictionary<Guid, GameState>()),
            hubContext,
            Options.Create(playersPool),
            Options.Create(playerConnections)
        );

        var whitePlayerId = Guid.NewGuid();
        var blackPlayerId = Guid.NewGuid();

        hubContext.Groups.Returns(groupClients);
        groupClients.AddToGroupAsync(Arg.Any<string>(), Arg.Any<string>(), cancellationTokenSource.Token).Returns(Task.CompletedTask);
        
        var clientProxy = Substitute.For<INotificationHub>();
        hubContext.Clients.Group(Arg.Any<string>()).Returns(clientProxy);

        // Act
        var result = await controller.StartNew(whitePlayerId, blackPlayerId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var gameId = Assert.IsType<Guid>(((ObjectResult)result).Value);
        Assert.NotEqual(Guid.Empty, gameId);
        await groupClients.Received(2).AddToGroupAsync(Arg.Any<string>(), Arg.Any<string>(), cancellationTokenSource.Token);
        await clientProxy.Received().OnGameStarted(Arg.Any<string>());
    }
}