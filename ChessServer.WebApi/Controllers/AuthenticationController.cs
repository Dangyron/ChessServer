using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.Authentication;
using ChessServer.Domain.DtoS;
using ChessServer.Domain.Models;
using ChessServer.WebApi.Authentication.Interfaces;
using ChessServer.WebApi.Controllers.Base;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ChessServer.WebApi.Controllers;

[Route("auth")]
public sealed class AuthenticationController : BaseController
{
    private readonly IMapper _mapper;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUserRepository _userRepository;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public AuthenticationController(IMapper mapper, IJwtTokenGenerator jwtTokenGenerator,
        IUserRepository userRepository, CancellationTokenSource cancellationTokenSource)
    {
        _mapper = mapper;
        _jwtTokenGenerator = jwtTokenGenerator;
        _userRepository = userRepository;
        _cancellationTokenSource = cancellationTokenSource;
    }

    [HttpPost("register"), AllowAnonymous]
    public async Task<IActionResult> Register([FromQuery] RegisterRequest request)
    {
        if (await _userRepository.GetByEmailAsync(request.Email) != null ||
            await _userRepository.GetByUsernameAsync(request.Username) != null)
            return Conflict("User already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Username = request.Username,
            Password = request.Password,
            Subscription = new Subscription(SubscriptionType.Basic),
            EloRating = 1500,
        };

        var token = _jwtTokenGenerator.Generate(_mapper.Map<UserJwtDto>(user));

        var json = JsonConvert.SerializeObject(_mapper.Map<PlayerDto>(user));

        await _userRepository.AddAsync(user, _cancellationTokenSource.Token);
        await _userRepository.SaveChangesAsync(_cancellationTokenSource.Token);
        return Ok(token);
    }

    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login([FromQuery] LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, _cancellationTokenSource.Token);
        if (user == null)
            return NotFound("User doesn't exist.");

        var token = _jwtTokenGenerator.Generate(_mapper.Map<UserJwtDto>(user));

        return Ok(token);
    }
}