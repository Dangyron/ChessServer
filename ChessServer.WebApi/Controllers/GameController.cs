using System.Collections.Concurrent;
using ChessLogic;
using ChessLogic.Moves;
using ChessLogic.Pieces;
using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.Dtos;
using ChessServer.Domain.Models;
using ChessServer.WebApi.Common;
using ChessServer.WebApi.Common.Extensions;
using ChessServer.WebApi.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ChessServer.WebApi.Controllers;

[Route("game")]
public class GameController : ControllerBase
{
    private readonly IGameRepository _gameRepository;
    private readonly IUserRepository _userRepository;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly IHubContext<NotificationHub, INotificationHub> _hubContext;
    private readonly ConcurrentDictionary<Guid, PlayingGame> _currentPlayingGames;
    private readonly ConcurrentDictionary<Guid, string> _playerConnections;
    private readonly ConcurrentBag<Guid> _playersPool;

    public GameController(IGameRepository gameRepository,
        CancellationTokenSource cancellationTokenSource,
        IOptions<ConcurrentDictionary<Guid, PlayingGame>> currentPlayingGames,
        IHubContext<NotificationHub, INotificationHub> hubContext,
        IOptions<ConcurrentBag<Guid>> playersPool,
        IOptions<ConcurrentDictionary<Guid, string>> playerConnections, IUserRepository userRepository)
    {
        _gameRepository = gameRepository;
        _cancellationTokenSource = cancellationTokenSource;
        _currentPlayingGames = currentPlayingGames.Value;
        _hubContext = hubContext;
        _userRepository = userRepository;
        _playerConnections = playerConnections.Value;
        _playersPool = playersPool.Value;
    }

    [HttpPost("start-new")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> StartNew()
    {
        var userId = User.GetId();

        if (_playersPool.Contains(userId))
            return Conflict();

        if (_playersPool.IsEmpty)
        {
            _playersPool.Add(userId);
            return Ok();
        }

        _playersPool.TryTake(out var opponent);

        return await StartNew(userId, opponent);
    }
    
    [HttpPost("reconnect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reconnect()
    {
        var userId = User.GetId();

        var game = _currentPlayingGames.FirstOrDefault(game =>
            game.Value.WhitePlayer == userId || game.Value.BlackPlayer == userId).Value;

        if (game is null)
            return BadRequest();
        

        _playersPool.TryPeek(out var opponent);

        return await StartNew(userId, opponent);
    }

    public async Task<IActionResult> StartNew(Guid whitePlayerId, Guid blackPlayerId, bool isRating = false)
    {
        var whitePlayer = await _userRepository.GetByIdAsync(whitePlayerId);
        var blackPlayer = await _userRepository.GetByIdAsync(blackPlayerId);
        
        var game = new Game
        {
            Id = Guid.NewGuid(),
            IsRating = isRating,
            WhitePlayerId = whitePlayerId,
            BlackPlayerId = blackPlayerId,
            Result = GameResult.Playing,
            StartTime = DateTime.UtcNow,
            Fen = GameState.DefaultFen,
        };

        _currentPlayingGames.TryAdd(game.Id, new(new GameState(), whitePlayerId, blackPlayerId));

        await _hubContext.Clients.Client(_playerConnections[whitePlayerId]).OnGameStarted(
            JsonConvert.SerializeObject(new GameStartedDto { GameId = game.Id, PlayerColor = PlayerColor.White, OpponentUsername = blackPlayer!.Username})
        );

        await _hubContext.Clients.Client(_playerConnections[blackPlayerId]).OnGameStarted(
            JsonConvert.SerializeObject(new GameStartedDto { GameId = game.Id, PlayerColor = PlayerColor.Black, OpponentUsername = whitePlayer!.Username})
        );

        await _hubContext.Groups.AddToGroupAsync(_playerConnections[whitePlayerId], game.Id.ToString(),
            _cancellationTokenSource.Token);
        await _hubContext.Groups.AddToGroupAsync(_playerConnections[blackPlayerId], game.Id.ToString(),
            _cancellationTokenSource.Token);

        await _gameRepository.AddAsync(game, _cancellationTokenSource.Token);
        await _gameRepository.SaveChangesAsync(_cancellationTokenSource.Token);

        return Ok();
    }

    [HttpPost("abort")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Abort(Guid gameId)
    {
        var gameState = _currentPlayingGames.GetValueOrDefault(gameId, PlayingGame.None);

        if (gameState.Game == null)
            return BadRequest("Game not found");

        await EndGame(gameId, GameResult.Aborted);

        return Ok();
    }

    [HttpPost("resign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Resign(Guid gameId)
    {
        var gameState = _currentPlayingGames.GetValueOrDefault(gameId, PlayingGame.None);

        if (gameState.Game == null)
            return BadRequest("Game not found");

        var userId = User.GetId();
        var game = (await _gameRepository.GetByIdAsync(gameId))!;

        var result = game.WhitePlayerId == userId ? GameResult.WhiteWin : GameResult.BlackWin;

        await EndGame(gameId, result);

        return Ok();
    }

    [HttpPost("move")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MakeMove([FromQuery] MoveRequest request)
    {
        var currentGame = _currentPlayingGames.GetValueOrDefault(request.Id, PlayingGame.None);

        if (currentGame.Game == null)
            return BadRequest("Game not found");

        var userId = User.GetId();
        var currentPlayer = currentGame.Game.CurrentPlayer == PlayerColor.White
            ? currentGame.WhitePlayer
            : currentGame.BlackPlayer;

        if (userId != currentPlayer)
            return BadRequest();

        var move = CreateMove(request, currentGame.Game);

        if (!currentGame.Game.MakeMove(move))
            return BadRequest();

        var moveResponse = new MoveResponse
        {
            Id = request.Id, 
            Move = currentGame.Game.ConvertMoveToString(move)
        };
        
        await _hubContext.Clients.Group(request.Id.ToString())
            .OnMoveReceived(JsonConvert.SerializeObject(moveResponse));

        if (!currentGame.Game.IsGameOver)
            return Ok();

        await EndGame(request.Id, currentGame.Game.Result!.ToGameResult());

        return Ok();
    }

    private static Move CreateMove(MoveRequest request, GameState currentGame)
    {
        var from = new Position(request.Move[..2].AsSpan());
        var to = new Position(request.Move[2..4].AsSpan());

        var move = currentGame.GetAllMovesForPlayer(currentGame.CurrentPlayer)
            .FirstOrDefault(mv => mv.From == from && mv.To == to, Move.None);

        if (move is Promotion)
        {
            return char.ToLower(request.Move[4]) switch
            {
                'n' => new Promotion(from, to, PieceType.Knight),
                'b' => new Promotion(from, to, PieceType.Bishop),
                'r' => new Promotion(from, to, PieceType.Rook),
                'q' => new Promotion(from, to, PieceType.Queen),
                _ => Move.None,
            };
        }

        return move;
    }

    private async Task EndGame(Guid gameId, GameResult result)
    {
        var game = (await _gameRepository.GetByIdAsync(gameId))!;
        game.Result = result;

        var gameEnded = new GameEndedDto
        {
            GameId = gameId,
            Result = result
        };

        _currentPlayingGames.TryRemove(gameId, out _);
        
        await _hubContext.Clients.Group(game.Id.ToString()).OnGameEnded(JsonConvert.SerializeObject(gameEnded));

        await _gameRepository.UpdateAsync(game, _cancellationTokenSource.Token);
        await _gameRepository.SaveChangesAsync(_cancellationTokenSource.Token);

        await _hubContext.Groups.RemoveFromGroupAsync(_playerConnections[game.WhitePlayerId], game.Id.ToString());
        await _hubContext.Groups.RemoveFromGroupAsync(_playerConnections[game.BlackPlayerId], game.Id.ToString());

        _currentPlayingGames.TryRemove(game.Id, out _);
        _playerConnections.TryRemove(game.WhitePlayerId, out _);
        _playerConnections.TryRemove(game.BlackPlayerId, out _);
    }
}