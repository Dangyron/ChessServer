using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.DtoS;
using ChessServer.WebApi.Common.Extensions;
using ChessServer.WebApi.Controllers.Base;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ChessServer.WebApi.Controllers;

[Route("[controller]")]
public class UserController : BaseController
{
    private readonly IUserRepository _userRepository;
    private readonly IGameRepository _gameRepository;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly IMapper _mapper;
    
    public UserController(IUserRepository userRepository, IMapper mapper, CancellationTokenSource cancellationTokenSource, IGameRepository gameRepository)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _cancellationTokenSource = cancellationTokenSource;
        _gameRepository = gameRepository;
    }

    [HttpGet("games")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<IActionResult> GetGames()
    {
        var userId = User.GetId();
        var games = _gameRepository.FindFor(userId, _cancellationTokenSource.Token);

        return Task.FromResult((IActionResult)Ok(games));
    }
    
    [HttpPut("add-info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AddInfo(AddUserInfoDto userInfoDto)
    {
        var userId = User.GetId();
        
        var user = (await _userRepository.GetByIdAsync(userId))!;

        user.Age = userInfoDto.Age ?? user.Age;
        user.Country = userInfoDto.Country ?? user.Country;
        user.Gender = userInfoDto.Gender ?? user.Gender;
        user.Email = userInfoDto.Email ?? user.Email;
        user.Username = userInfoDto.Username ?? user.Username;
        user.Password = userInfoDto.Password ?? user.Password;
        
        await _userRepository.UpdateAsync(user, _cancellationTokenSource.Token);
        await _userRepository.SaveChangesAsync(_cancellationTokenSource.Token);
        
        return Ok();
    }
    
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInfo()
    {
        var userId = User.GetId();
        var user = await _userRepository.GetByIdAsync(userId);
        return Ok(JsonConvert.SerializeObject(user));
    }
    
    [HttpDelete("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete()
    {
        var userId = User.GetId();
        var user = await _userRepository.GetByIdAsync(userId);
        _userRepository.Remove(user!);
        return Ok();
    }
}