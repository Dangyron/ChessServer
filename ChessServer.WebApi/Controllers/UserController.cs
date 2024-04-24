using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.Models;
using ChessServer.WebApi.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ChessServer.WebApi.Controllers;

[Route("[controller]")]
public class UserController : BaseController
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet("games")]
    public Task<IActionResult> GetGames()
    {
        return Task.FromResult((IActionResult)Ok());
    }
    
    [HttpPost("add-info")]
    public async Task<IActionResult> AddInfo(User user)
    {
        await _userRepository.UpdateAsync(user);
        return Ok();
    }
    
    [HttpPost("info")]
    public async Task<IActionResult> GetInfo(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return Ok(JsonConvert.SerializeObject(user));
    }
    
    [HttpDelete("delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _userRepository.Remove((await _userRepository.GetByIdAsync(id))!);
        return Ok();
    }
}