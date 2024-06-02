using System.Collections.Concurrent;
using System.Security.Claims;
using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.Dtos;
using ChessServer.Domain.Models;
using ChessServer.WebApi.Authentication;
using ChessServer.WebApi.Common;
using ChessServer.WebApi.Common.Interfaces;
using ChessServer.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        var playersPool = new ConcurrentDictionary<Guid, bool>();
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
            new ConcurrentDictionary<Guid, PlayingGame>(),
            hubContext,
            playersPool,
            playerConnections,
            Substitute.For<IUserRepository>()
        )
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
        };

        // Act
        var result = await controller.StartNew();

        // Assert
        Assert.IsType<OkResult>(result);
        Assert.Contains(userId, playersPool.Keys);
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
        var userRepository = Substitute.For<IUserRepository>();
        
        var controller = new GameController(
            Substitute.For<IGameRepository>(),
            cancellationTokenSource,
            new ConcurrentDictionary<Guid, PlayingGame>(),
            hubContext,
            new ConcurrentDictionary<Guid, bool>(),
            playerConnections,
            userRepository
        );

        var whitePlayerId = Guid.NewGuid();
        var blackPlayerId = Guid.NewGuid();

        playerConnections[whitePlayerId] = string.Empty;
        playerConnections[blackPlayerId] = string.Empty;

        hubContext.Groups.Returns(groupClients);
        groupClients.AddToGroupAsync(Arg.Any<string>(), Arg.Any<string>(), cancellationTokenSource.Token)
            .Returns(Task.CompletedTask);

        hubContext.Clients.Returns(hubClients);
        hubClients.Client(Arg.Any<string>()).Returns(hubClient);

        var whitePlayer = new User { Id = whitePlayerId, Username = "WhitePlayer" };
        var blackPlayer = new User { Id = blackPlayerId, Username = "BlackPlayer" };
        
        userRepository.GetByIdAsync(whitePlayerId, cancellationTokenSource.Token)
            .Returns(Task.FromResult<User?>(whitePlayer));
        userRepository.GetByIdAsync(blackPlayerId, cancellationTokenSource.Token)
            .Returns(Task.FromResult<User?>(blackPlayer));
        // Act
        var result = await controller.StartNew(whitePlayerId, blackPlayerId);

        // Assert
        Assert.IsType<OkResult>(result);
        await hubClient.Received(2).OnGameStarted(Arg.Any<string>());
        await groupClients.Received(2)
            .AddToGroupAsync(Arg.Any<string>(), Arg.Any<string>(), cancellationTokenSource.Token);
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
            new ConcurrentDictionary<Guid, PlayingGame>(),
            hubContext,
            new ConcurrentDictionary<Guid, bool>(),
            playerConnections,
            Substitute.For<IUserRepository>()
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
            new ConcurrentDictionary<Guid, PlayingGame>(),
            hubContext,
            new ConcurrentDictionary<Guid, bool>(),
            playerConnections,
            Substitute.For<IUserRepository>()
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
        var httpContext = Substitute.For<HttpContext>();
        var userId = Guid.NewGuid();
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(CustomClaims.UserId, userId.ToString())
        }));

        httpContext.User.Returns(claimsPrincipal);
        var gameId = Guid.NewGuid();
        var currentlyPlayingGames = new ConcurrentDictionary<Guid, PlayingGame>
        {
            [gameId] = new(new(), userId, Guid.Empty)
        };

        var controller = new GameController(
            Substitute.For<IGameRepository>(),
            cancellationTokenSource,
            currentlyPlayingGames,
            hubContext,
            new ConcurrentDictionary<Guid, bool>(),
            playerConnections,
            Substitute.For<IUserRepository>()
        )
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
        };

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