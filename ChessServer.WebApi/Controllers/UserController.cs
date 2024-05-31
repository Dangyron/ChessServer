﻿using System.Collections.Concurrent;
using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.Dtos;
using ChessServer.Domain.Models;
using ChessServer.WebApi.Common.Extensions;
using ChessServer.WebApi.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ChessServer.WebApi.Controllers;

[Route("[controller]")]
public class UserController : BaseController
{
    private readonly IUserRepository _userRepository;
    private readonly IGameRepository _gameRepository;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ConcurrentDictionary<Guid, PlayingGame> _currentPlayingGames;

    public UserController(IUserRepository userRepository, CancellationTokenSource cancellationTokenSource,
        IGameRepository gameRepository, IOptions<ConcurrentDictionary<Guid, PlayingGame>> currentPlayingGames)
    {
        _userRepository = userRepository;
        _cancellationTokenSource = cancellationTokenSource;
        _gameRepository = gameRepository;
        _currentPlayingGames = currentPlayingGames.Value;
    }

    [HttpGet("games")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGames()
    {
        var userId = User.GetId();
        var games = await _gameRepository.FindFor(userId, _cancellationTokenSource.Token);

        return Ok(games);
    }
    
    [HttpGet("current-playing-games")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<OkObjectResult> GetCurrentPlayingGames()
    {
        var userId = User.GetId();
        var game = _currentPlayingGames.Where(game =>
            game.Value.WhitePlayer == userId || game.Value.BlackPlayer == userId).Select(game => game.Value);

        return Task.FromResult(Ok(game));
    }

    [HttpPut("add-info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AddInfo(AddUserInfoDto userInfoDto)
    {
        var userId = User.GetId();

        var user = (await _userRepository.GetByIdAsync(userId))!;

        user.Age = userInfoDto.Age ?? user.Age;
        user.Country = userInfoDto.Country ?? user.Country;
        if (userInfoDto.Gender is not null && Gender.TryFromValue(userInfoDto.Gender.Value, out var gender))
        {
            user.Gender = gender;
        }

        user.Email = userInfoDto.Email ?? user.Email;
        user.Username = userInfoDto.Username ?? user.Username;
        user.Password = userInfoDto.Password ?? user.Password;

        await _userRepository.UpdateAsync(user, _cancellationTokenSource.Token);
        await _userRepository.SaveChangesAsync(_cancellationTokenSource.Token);

        return Ok(user);
    }

    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInfo()
    {
        var userId = User.GetId();
        var user = await _userRepository.GetByIdAsync(userId, _cancellationTokenSource.Token);
        return Ok(user);
    }

    [HttpDelete("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete()
    {
        var userId = User.GetId();
        var user = await _userRepository.GetByIdAsync(userId, _cancellationTokenSource.Token);
        _userRepository.Remove(user!);
        return Ok();
    }
}