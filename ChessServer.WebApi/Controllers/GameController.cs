using System.Collections.Concurrent;
using ChessLogic;
using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.DtoS;
using ChessServer.Domain.Models;
using ChessServer.WebApi.Common;
using ChessServer.WebApi.Common.Extensions;
using ChessServer.WebApi.Common.Interfaces;
using ChessServer.WebApi.Controllers.Base;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ChessServer.WebApi.Controllers;

[Route("game")]
public class GameController : BaseController
{
    private readonly IGameRepository _gameRepository;
    private readonly IUserRepository _userRepository;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly IMapper _mapper;
    private readonly IHubContext<NotificationHub, INotificationHub> _hubContext;
    private readonly ConcurrentDictionary<Guid, GameState> _currentPlayingGames;
    private readonly ConcurrentDictionary<Guid, string> _playerConnections;
    private readonly ConcurrentBag<Guid> _playersPool;

    public GameController(IGameRepository gameRepository, IUserRepository userRepository,
        CancellationTokenSource cancellationTokenSource, IMapper mapper,
        IOptions<ConcurrentDictionary<Guid, GameState>> currentPlayingGames,
        IHubContext<NotificationHub, INotificationHub> hubContext, IOptions<ConcurrentBag<Guid>> playersPool,
        IOptions<ConcurrentDictionary<Guid, string>> playerConnections)
    {
        _gameRepository = gameRepository;
        _userRepository = userRepository;
        _cancellationTokenSource = cancellationTokenSource;
        _mapper = mapper;
        _currentPlayingGames = currentPlayingGames.Value;
        _hubContext = hubContext;
        _playerConnections = playerConnections.Value;
        _playersPool = playersPool.Value;
    }

    [HttpPost("start-new")]
    public Task<IActionResult> AddToPool()
    {
        var userId = User.GetId();

        if (_playersPool.Contains(userId))
            return Task.FromResult((IActionResult)Ok());

        if (_playersPool.IsEmpty)
        {
            _playersPool.Add(userId);
            return Task.FromResult((IActionResult)Ok());
        }

        _playersPool.TryPeek(out var opponent);

        var value = new
        {
            WhitePlayerId = userId,
            BlackPlayerId = opponent
        };

        return Task.FromResult((IActionResult)RedirectToAction(nameof(StartNew), value));
    }

    public async Task<IActionResult> StartNew(Guid whitePlayerId, Guid blackPlayerId, bool isRating = false)
    {
        var game = new Game
        {
            Id = Guid.NewGuid(),
            IsRating = isRating,
            WhitePlayerId = whitePlayerId,
            BlackPlayerId = blackPlayerId,
            Result = GameResult.Playing,
            StartTime = DateTime.UtcNow,
            Fen = Game.DefaultFen
        };

        _currentPlayingGames.TryAdd(game.Id, new GameState());

        await _hubContext.Groups.AddToGroupAsync(_playerConnections[whitePlayerId], game.Id.ToString());
        await _hubContext.Groups.AddToGroupAsync(_playerConnections[blackPlayerId], game.Id.ToString());

        await _gameRepository.AddAsync(game, _cancellationTokenSource.Token);
        await _gameRepository.SaveChangesAsync(_cancellationTokenSource.Token);

        return Ok(game.Id);
    }

    [HttpPost("abort")]
    public async Task<IActionResult> Abort(Guid gameId)
    {
        var gameState = _currentPlayingGames.GetValueOrDefault(gameId);

        if (gameState == null)
            return BadRequest("Game not found");

        var game = (await _gameRepository.GetByIdAsync(gameId))!;

        game.Result = GameResult.Abort;
        _currentPlayingGames.TryRemove(gameId, out _);
        
        await _gameRepository.UpdateAsync(game, _cancellationTokenSource.Token);
        await _gameRepository.SaveChangesAsync(_cancellationTokenSource.Token);

        return Ok();
    }

    [HttpPost("move")]
    public async Task<IActionResult> MakeMove(MoveRequest request)
    {
        var gameState = _currentPlayingGames.GetValueOrDefault(request.Id);

        if (gameState == null)
            return BadRequest("Game not found");

        if (!gameState.MakeMove(request.Move))
        {
            return BadRequest();
        }
        
        await _hubContext.Clients.Group(request.Id.ToString()).OnMoveSent(JsonConvert.SerializeObject(request));

        if (!gameState.IsGameOver)
            return Ok();
        
        return Ok();
    }
}