using System.Collections.Concurrent;
using System.Security.Claims;
using ChessLogic;
using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.DtoS;
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
        var cancellationTokenSource = new CancellationTokenSource();
        var playerConnections = new ConcurrentDictionary<Guid, string>();
        var groupClients = Substitute.For<IGroupManager>();
        var hubClients = Substitute.For<IHubClients<INotificationHub>>();
        var hubContext = Substitute.For<IHubContext<NotificationHub, INotificationHub>>();
        var hubClient = Substitute.For<INotificationHub>();
        
        var controller = new GameController(
            Substitute.For<IGameRepository>(),
            cancellationTokenSource,
            Options.Create(new ConcurrentDictionary<Guid, GameState>()),
            hubContext,
            Options.Create(new ConcurrentBag<Guid>()),
            Options.Create(playerConnections)
        );

        var whitePlayerId = Guid.NewGuid();
        var blackPlayerId = Guid.NewGuid();

        playerConnections[whitePlayerId] = string.Empty;
        playerConnections[blackPlayerId] = string.Empty;

        hubContext.Groups.Returns(groupClients);
        groupClients.AddToGroupAsync(Arg.Any<string>(), Arg.Any<string>(), cancellationTokenSource.Token).Returns(Task.CompletedTask);
        
        hubContext.Clients.Returns(hubClients);
        hubClients.Client(Arg.Any<string>()).Returns(hubClient);
        
        // Act
        var result = await controller.StartNew(whitePlayerId, blackPlayerId);

        // Assert
        Assert.IsType<OkResult>(result);
        await hubClient.Received(2).OnGameStarted(Arg.Any<string>());
        await groupClients.Received(2).AddToGroupAsync(Arg.Any<string>(), Arg.Any<string>(), cancellationTokenSource.Token);
    }
    
    [Fact]
    public async Task Abort_WithInvalidId_ReturnBadRequestObjectResult()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var playerConnections = new ConcurrentDictionary<Guid, string>();
        var hubContext = Substitute.For<IHubContext<NotificationHub, INotificationHub>>();
        var controller = new GameController(
            Substitute.For<IGameRepository>(),
            cancellationTokenSource,
            Options.Create(new ConcurrentDictionary<Guid, GameState>()),
            hubContext,
            Options.Create(new ConcurrentBag<Guid>()),
            Options.Create(playerConnections)
        );
        
        // Act
        var result = await controller.Abort(Guid.Empty);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
    
    [Fact]
    public async Task Resign_WithInvalidId_ReturnBadRequestObjectResult()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var playerConnections = new ConcurrentDictionary<Guid, string>();
        var hubContext = Substitute.For<IHubContext<NotificationHub, INotificationHub>>();
        var controller = new GameController(
            Substitute.For<IGameRepository>(),
            cancellationTokenSource,
            Options.Create(new ConcurrentDictionary<Guid, GameState>()),
            hubContext,
            Options.Create(new ConcurrentBag<Guid>()),
            Options.Create(playerConnections)
        );
        
        // Act
        var result = await controller.Resign(Guid.Empty);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
    
    [Fact]
    public async Task MakeMove_WithValidPlayers_UpdateCurrantGameAndNotifyPlayers()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var playerConnections = new ConcurrentDictionary<Guid, string>();
        var hubClients = Substitute.For<IHubClients<INotificationHub>>();
        var hubContext = Substitute.For<IHubContext<NotificationHub, INotificationHub>>();
        var hubClient = Substitute.For<INotificationHub>();
        var gameId = Guid.NewGuid();
        var currentlyPlayingGames = new ConcurrentDictionary<Guid, GameState>
        {
            [gameId] = new()
        };

        var controller = new GameController(
            Substitute.For<IGameRepository>(),
            cancellationTokenSource,
            Options.Create(currentlyPlayingGames),
            hubContext,
            Options.Create(new ConcurrentBag<Guid>()),
            Options.Create(playerConnections)
        );

        var moveRequest = new MoveRequest
        {
            Id = gameId,
            Move = "e2e4",
        };
        
        hubContext.Clients.Returns(hubClients);
        hubClients.Group(Arg.Any<string>()).Returns(hubClient);
        
        // Act
        var result = await controller.MakeMove(moveRequest);

        // Assert
        Assert.IsType<OkResult>(result);
        await hubClient.Received(1).OnMoveReceived(Arg.Any<string>());
    }
}