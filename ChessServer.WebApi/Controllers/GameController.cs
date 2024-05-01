using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.DtoS;
using ChessServer.Domain.Models;
using ChessServer.WebApi.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChessServer.WebApi.Controllers;

[Route("game")]
public class GameController : BaseController
{
    private readonly IGameRepository _gameRepository;
    private readonly IUserRepository _userRepository;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public GameController(IGameRepository gameRepository, IUserRepository userRepository, CancellationTokenSource cancellationTokenSource)
    {
        _gameRepository = gameRepository;
        _userRepository = userRepository;
        _cancellationTokenSource = cancellationTokenSource;
    }

    [HttpPost("start-new")]
    public async Task<IActionResult> StartNew(Guid whitePlayerId, Guid blackPlayerId, bool isRating = false)
    {
        var whitePlayer = await _userRepository.GetByIdAsync(whitePlayerId, _cancellationTokenSource.Token);
        var blackPlayer = await _userRepository.GetByIdAsync(blackPlayerId, _cancellationTokenSource.Token);

        if (whitePlayer == null || blackPlayer == null)
            return BadRequest("Invalid users");

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

        await _gameRepository.AddAsync(game, _cancellationTokenSource.Token);
        await _gameRepository.SaveChangesAsync(_cancellationTokenSource.Token);

        return Ok();
    }
    
    [HttpPost("abort")]
    public async Task<IActionResult> Abort(Guid gameId)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, _cancellationTokenSource.Token);

        if (game == null)
            return BadRequest("Game not found");

        game.Result = GameResult.Abort;
        
        await _gameRepository.UpdateAsync(game, _cancellationTokenSource.Token);
        await _gameRepository.SaveChangesAsync(_cancellationTokenSource.Token);
        
        return Ok();
    }

    [HttpPost("move")]
    public async Task<IActionResult> Move(GameMoveRequest request)
    {
        var game = await _gameRepository.GetAsync(g => g.Id == request.Id, _cancellationTokenSource.Token);

        if (game == null)
            return NotFound();

        game.Fen = request.Fen;
        
        await _gameRepository.UpdateAsync(game, _cancellationTokenSource.Token);
        await _gameRepository.SaveChangesAsync(_cancellationTokenSource.Token);
        
        return Ok(game);
    }
}