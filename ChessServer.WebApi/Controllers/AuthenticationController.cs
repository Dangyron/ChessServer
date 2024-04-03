using ChessServer.Domain.Authentication;
using ChessServer.Domain.Models;
using ChessServer.WebApi.Controllers.Base;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedLibrary.Models;

namespace ChessServer.WebApi.Controllers;

[Route("auth")]
public sealed class AuthenticationController : BaseController
{
    private readonly IMapper _mapper;

    public AuthenticationController(IMapper mapper)
    {
        _mapper = mapper;
    }
    
    [HttpPost("register")]
    public IActionResult Register([FromQuery] RegisterRequest request)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.UserName,
            Password = request.Password,
        };

        var json = JsonConvert.SerializeObject(_mapper.Map<Player>(user));

        return Ok(json);
    }
        
    [HttpPost("login")]
    public IActionResult Login([FromQuery] LoginRequest request)
    {
        var user = new User
        {
            UserName = request.UserName,
            Password = request.Password,
        };

        var json = JsonConvert.SerializeObject(_mapper.Map<Player>(user));

        return Ok(json);
    }
    
}